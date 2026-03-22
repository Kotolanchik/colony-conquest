using ColonyConquest.Analytics;
using ColonyConquest.Core;
using ColonyConquest.Ecology;
using ColonyConquest.Economy;
using ColonyConquest.Entertainment;
using ColonyConquest.Housing;
using ColonyConquest.Justice;
using ColonyConquest.Simulation;
using ColonyConquest.Story;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Settlers
{
    /// <summary>
    /// Суточный runtime-контур поселенцев: нужды, психология, физиология, навыки, автономия, демография и интеграции.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ColonyCalendarAdvanceSystem))]
    [UpdateAfter(typeof(EntertainmentDailySystem))]
    [UpdateAfter(typeof(HousingDailyComfortSystem))]
    [UpdateBefore(typeof(AnalyticsSnapshotUpdateSystem))]
    public partial struct SettlerSimulationDailySystem : ISystem
    {
        private const uint EventMentalBreak = 0xE601;
        private const uint EventSettlerDeath = 0xE602;
        private const uint EventSettlerBirth = 0xE603;
        private const uint EventNeedCritical = 0xE604;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameCalendarState>();
            state.RequireForUpdate<SimulationRootState>();
            state.RequireForUpdate<SettlerSimulationSingleton>();
            state.RequireForUpdate<SettlerSimulationState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var day = SystemAPI.GetSingleton<GameCalendarState>().DayIndex;
            var tick = SystemAPI.GetSingleton<SimulationRootState>().SimulationTick;
            ref var sim = ref SystemAPI.GetSingletonRW<SettlerSimulationState>().ValueRW;
            if (sim.LastProcessedDay == day)
                return;
            sim.LastProcessedDay = day;

            var entertainmentMood = SystemAPI.HasSingleton<EntertainmentSimulationState>()
                ? SystemAPI.GetSingleton<EntertainmentSimulationState>().FinalMood
                : 50f;
            var entertainmentStressReduction = SystemAPI.HasSingleton<EntertainmentSimulationState>()
                ? SystemAPI.GetSingleton<EntertainmentSimulationState>().StressReduction
                : 0f;
            var housingComfort = SystemAPI.HasSingleton<HousingColonyState>()
                ? SystemAPI.GetSingleton<HousingColonyState>().AverageComfort
                : 50f;
            var colonyOvercrowdingUnits = SystemAPI.HasSingleton<HousingColonyState>()
                ? SystemAPI.GetSingleton<HousingColonyState>().OvercrowdedUnits
                : 0;
            var pollutionBand = SystemAPI.HasSingleton<ColonyPollutionSummaryState>()
                ? SystemAPI.GetSingleton<ColonyPollutionSummaryState>().Band
                : PollutionLevelBand.Low;
            var pollutionHealthMultiplier = EcologyPollutionMath.GetPopulationHealthMultiplier(pollutionBand);
            var pollutionMoodMultiplier = EcologyPollutionMath.GetColonyMoodMultiplier(pollutionBand);

            sim.WaterReserveUnits = math.min(10000f, sim.WaterReserveUnits + 30f + housingComfort * 0.15f);

            var hasStockpile = SystemAPI.HasSingleton<ResourceStockpileSingleton>();
            DynamicBuffer<ResourceStockEntry> stockpile = default;
            if (hasStockpile)
                stockpile = SystemAPI.GetSingletonBuffer<ResourceStockEntry>(ref state);

            var traitsLookup = SystemAPI.GetComponentLookup<PersonalityTraits>(true);
            var aptitudesLookup = SystemAPI.GetComponentLookup<Aptitudes>(true);
            var moodLookup = SystemAPI.GetComponentLookup<MoodModifiers>(false);
            var mentalLookup = SystemAPI.GetComponentLookup<MentalConditions>(false);
            var skillLookup = SystemAPI.GetComponentLookup<SkillSet>(false);
            var usageLookup = SystemAPI.GetComponentLookup<SkillUsageTracker>(false);
            var injuryLookup = SystemAPI.GetComponentLookup<InjuryTracker>(false);
            var medicalLookup = SystemAPI.GetComponentLookup<MedicalConditions>(false);
            var socialLookup = SystemAPI.GetComponentLookup<SocialBonds>(false);
            var autonomyLookup = SystemAPI.GetComponentLookup<AutonomyLevel>(false);
            var taskLookup = SystemAPI.GetComponentLookup<CurrentTask>(false);
            var aiLookup = SystemAPI.GetComponentLookup<AIState>(false);
            var hierarchyLookup = SystemAPI.GetComponentLookup<CommandHierarchy>(false);

            var deadLookup = SystemAPI.GetComponentLookup<IsDead>(false);
            var hungryLookup = SystemAPI.GetComponentLookup<IsHungry>(false);
            var exhaustedLookup = SystemAPI.GetComponentLookup<IsExhausted>(false);
            var woundedLookup = SystemAPI.GetComponentLookup<IsWounded>(false);
            var infectedLookup = SystemAPI.GetComponentLookup<IsInfected>(false);
            var breakLookup = SystemAPI.GetComponentLookup<HasMentalBreak>(false);
            var sleepingLookup = SystemAPI.GetComponentLookup<IsSleeping>(false);
            var combatLookup = SystemAPI.GetComponentLookup<IsInCombat>(true);
            var draftedLookup = SystemAPI.GetComponentLookup<IsDrafted>(true);
            var incapacitatedLookup = SystemAPI.GetComponentLookup<IsIncapacitated>(false);

            var edges = SystemAPI.GetSingletonBuffer<SettlerRelationshipEdge>(ref state);
            edges.Clear();

            uint populationAlive = 0;
            uint deathsToday = 0;
            uint breaksToday = 0;
            uint hungryToday = 0;
            uint exhaustedToday = 0;
            uint infectedToday = 0;
            uint criticalNeedToday = 0;
            float foodDemand = 0f;
            float foodConsumed = 0f;
            float moodSum = 0f;
            float stressSum = 0f;
            float health01Sum = 0f;
            float workEfficiencySum = 0f;
            float educationSkillSum = 0f;
            uint educationSettlers = 0;
            uint scientistCount = 0;
            var policy = SystemAPI.GetSingleton<SettlerAutonomyPolicyState>();

            foreach (var (runtimeId, identityRw, psychRw, physioRw, needsRw, lifecycleRw, statsRw, runtimeRw, entity)
                     in SystemAPI
                         .Query<RefRO<SettlerRuntimeId>, RefRW<SettlerIdentity>, RefRW<PsychologyState>,
                             RefRW<PhysiologyState>, RefRW<NeedsState>, RefRW<LifecycleState>, RefRW<SettlerStats>,
                             RefRW<SettlerRuntimeState>>()
                         .WithEntityAccess())
            {
                var isDead = deadLookup.HasComponent(entity) && deadLookup.IsComponentEnabled(entity);
                if (isDead)
                    continue;

                ref var identity = ref identityRw.ValueRW;
                ref var psych = ref psychRw.ValueRW;
                ref var physio = ref physioRw.ValueRW;
                ref var needs = ref needsRw.ValueRW;
                ref var lifecycle = ref lifecycleRw.ValueRW;
                ref var stats = ref statsRw.ValueRW;
                ref var runtime = ref runtimeRw.ValueRW;

                var traits = traitsLookup.HasComponent(entity) ? traitsLookup[entity] : default;
                var aptitudes = aptitudesLookup.HasComponent(entity) ? aptitudesLookup[entity] : default;
                var mood = moodLookup.HasComponent(entity) ? moodLookup[entity] : default;
                var mental = mentalLookup.HasComponent(entity) ? mentalLookup[entity] : default;
                var injury = injuryLookup.HasComponent(entity) ? injuryLookup[entity] : default;
                var medical = medicalLookup.HasComponent(entity) ? medicalLookup[entity] : default;
                var social = socialLookup.HasComponent(entity) ? socialLookup[entity] : default;
                var autonomy = autonomyLookup.HasComponent(entity) ? autonomyLookup[entity] : default;
                var task = taskLookup.HasComponent(entity) ? taskLookup[entity] : default;
                var ai = aiLookup.HasComponent(entity) ? aiLookup[entity] : default;
                var hierarchy = hierarchyLookup.HasComponent(entity) ? hierarchyLookup[entity] : default;
                var hasSkillsComp = skillLookup.HasComponent(entity) && usageLookup.HasComponent(entity);
                var skills = hasSkillsComp ? skillLookup[entity] : default;
                var usage = hasSkillsComp ? usageLookup[entity] : default;

                var random = Random.CreateFromIndex(
                    math.max(1u, day * 1103u + runtimeId.ValueRO.Value * 977u + identity.GenerationSeed));

                var inCombat = combatLookup.HasComponent(entity) && combatLookup.IsComponentEnabled(entity);
                var drafted = draftedLookup.HasComponent(entity) && draftedLookup.IsComponentEnabled(entity);
                var sleeping = sleepingLookup.HasComponent(entity) && sleepingLookup.IsComponentEnabled(entity);

                const float FoodPerSettler = 2f;
                const float WaterPerSettler = 3f;
                foodDemand += FoodPerSettler;
                var consumedFood = hasStockpile ? ConsumeFoodRation(ref stockpile, FoodPerSettler) : 0f;
                foodConsumed += consumedFood;
                var foodSatisfied = consumedFood / FoodPerSettler;
                var consumedWater = math.min(sim.WaterReserveUnits, WaterPerSettler);
                sim.WaterReserveUnits -= consumedWater;
                var waterSatisfied = consumedWater / WaterPerSettler;

                UpdateNeeds(ref needs, task.Priority, inCombat, sleeping, foodSatisfied, waterSatisfied, housingComfort,
                    colonyOvercrowdingUnits);
                var needPenalty = SettlerSimulationMath.ComputeNeedMoodPenalty(needs);
                if (needs.Hunger > 90f || needs.Thirst > 90f || needs.Rest > 95f)
                    criticalNeedToday++;

                var socialSupport01 = UpdateSocialAndRelationships(ref social, ref sim, day, runtimeId.ValueRO.Value, ref edges);
                var activeMoodModifiers = TickMoodModifiers(ref mood);
                var prevMood = psych.Mood;
                var environmentMood =
                    (entertainmentMood - 50f) * 0.12f + (housingComfort - 50f) * 0.10f + socialSupport01 * 8f;
                psych.Mood = math.clamp(
                    mood.BaseMood + activeMoodModifiers + environmentMood - needPenalty - mental.DepressionLevel * 0.12f +
                    (pollutionMoodMultiplier - 1f) * 25f +
                    random.NextFloat(-2f, 2f), -100f, 100f);
                psych.MoodTrend = psych.Mood - prevMood;

                var hasInjury = injury.InjuryCount > 0 || injury.HasCriticalInjury;
                var phobiaPressure = mental.Phobias.Length > 0 ? mental.Phobias[0].Severity / 10f : 0f;
                var stressAccumulation = SettlerSimulationMath.ComputeStressAccumulation(
                    needs, physio, mental, inCombat, hasInjury, phobiaPressure);
                var stressRecovery = SettlerSimulationMath.ComputeStressRecovery(
                    needs.Recreation, sleeping, socialSupport01, entertainmentStressReduction / 100f);
                psych.Stress = math.clamp(psych.Stress + stressAccumulation - stressRecovery, 0f, 130f);
                psych.MentalBreakThreshold = SettlerSimulationMath.ComputeBreakThreshold(psych.Mood, mental.PTSDLevel);
                psych.CurrentBreakRisk = SettlerSimulationMath.ResolveBreakRisk(psych.Stress, psych.MentalBreakThreshold);

                if (psych.CurrentBreakRisk >= 3 && (!breakLookup.HasComponent(entity) || !breakLookup.IsComponentEnabled(entity)))
                {
                    if (random.NextFloat() < 0.35f)
                    {
                        if (breakLookup.HasComponent(entity))
                            breakLookup.SetComponentEnabled(entity, true);
                        psych.ActiveBreakType = psych.Stress >= 100f ? (byte)4 : psych.Stress >= 90f ? (byte)3 : (byte)2;
                        task.TaskType = 9;
                        task.Priority = 10;
                        breaksToday++;
                        sim.MentalBreaksTotal++;
                        stats.MentalBreaksCount++;
                        runtime.DaysSinceLastBreak = 0;
                        TryEnqueueStoryEvent(ref state, tick, EventMentalBreak, new FixedString64Bytes("settler-break"));
                    }
                }
                else if (breakLookup.HasComponent(entity) && psych.CurrentBreakRisk <= 1)
                {
                    breakLookup.SetComponentEnabled(entity, false);
                    psych.ActiveBreakType = 0;
                }

                UpdatePhysiology(ref physio, ref needs, ref injury, ref medical, in traits, in mental,
                    pollutionHealthMultiplier, ref random);

                var deadNow = physio.Health <= 0f || physio.BloodVolume < 15f;
                if (deadNow)
                {
                    moodLookup[entity] = mood;
                    mentalLookup[entity] = mental;
                    injuryLookup[entity] = injury;
                    medicalLookup[entity] = medical;
                    socialLookup[entity] = social;
                    autonomyLookup[entity] = autonomy;
                    taskLookup[entity] = task;
                    aiLookup[entity] = ai;

                    if (deadLookup.HasComponent(entity))
                        deadLookup.SetComponentEnabled(entity, true);
                    if (incapacitatedLookup.HasComponent(entity))
                        incapacitatedLookup.SetComponentEnabled(entity, true);
                    lifecycle.DeathTick = (int)tick;
                    lifecycle.DeathCause = physio.BloodVolume < 15f ? (byte)2 : (byte)1;
                    deathsToday++;
                    sim.DeathsTotal++;
                    TryEnqueueStoryEvent(ref state, tick, EventSettlerDeath, new FixedString64Bytes("settler-death"));
                    continue;
                }

                if (day > runtime.BirthDay && day % GameCalendarTuning.DaysPerGameYear == 0 && identity.Age < 80)
                    identity.Age++;
                lifecycle.LifeStage = identity.Age < 18 ? (byte)0 : identity.Age < 60 ? (byte)1 : (byte)2;

                if (hungryLookup.HasComponent(entity))
                    hungryLookup.SetComponentEnabled(entity, needs.Hunger > 50f);
                if (exhaustedLookup.HasComponent(entity))
                    exhaustedLookup.SetComponentEnabled(entity, needs.Rest > 70f);
                if (woundedLookup.HasComponent(entity))
                    woundedLookup.SetComponentEnabled(entity, injury.InjuryCount > 0 || physio.Health < 70f);
                if (infectedLookup.HasComponent(entity))
                    infectedLookup.SetComponentEnabled(entity, HasActiveInfection(in medical));
                if (sleepingLookup.HasComponent(entity))
                    sleepingLookup.SetComponentEnabled(entity, needs.Rest > 92f || (sleeping && needs.Rest > 20f));
                if (incapacitatedLookup.HasComponent(entity))
                {
                    incapacitatedLookup.SetComponentEnabled(entity, physio.Health < 25f || physio.Consciousness < 25f ||
                                                                    physio.Mobility < 20f);
                }

                if (hungryLookup.HasComponent(entity) && hungryLookup.IsComponentEnabled(entity))
                    hungryToday++;
                if (exhaustedLookup.HasComponent(entity) && exhaustedLookup.IsComponentEnabled(entity))
                    exhaustedToday++;
                if (infectedLookup.HasComponent(entity) && infectedLookup.IsComponentEnabled(entity))
                    infectedToday++;

                var passionBonus = 0f;
                if (hasSkillsComp)
                {
                    UpdateSkillProgress(ref skills, ref usage, in aptitudes, ref task, ref stats, in psych, day, ref random,
                        out passionBonus);
                    skillLookup[entity] = skills;
                    usageLookup[entity] = usage;
                }

                var workEfficiency = SettlerSimulationMath.ComputeWorkEfficiency(
                    physio,
                    psych.Mood,
                    psych.Stress,
                    passionBonus,
                    hierarchy.LeadershipBonus);
                if (breakLookup.HasComponent(entity) && breakLookup.IsComponentEnabled(entity))
                    workEfficiency *= 0.3f;
                runtime.WorkContributionToday = workEfficiency;
                stats.TotalWorkDone += workEfficiency;
                if (task.TaskType == 1 && workEfficiency > 0.8f && random.NextFloat() < 0.25f)
                    stats.BuildingsBuilt++;
                if (task.TaskType == 2 && workEfficiency > 0.7f && random.NextFloat() < 0.25f)
                    stats.ItemsCrafted++;
                if (task.TaskType == 3)
                    stats.ResearchPoints += (uint)math.round(workEfficiency * 2f);
                if (inCombat)
                    stats.BattlesParticipated++;

                UpdateAutonomyAndAi(ref autonomy, ref ai, in policy, in task, in hierarchy, in psych, in physio, drafted,
                    inCombat, tick, ref random);

                moodLookup[entity] = mood;
                mentalLookup[entity] = mental;
                injuryLookup[entity] = injury;
                medicalLookup[entity] = medical;
                socialLookup[entity] = social;
                autonomyLookup[entity] = autonomy;
                taskLookup[entity] = task;
                aiLookup[entity] = ai;

                populationAlive++;
                moodSum += psych.Mood;
                stressSum += psych.Stress;
                health01Sum += math.saturate(physio.Health / math.max(1f, physio.MaxHealth));
                workEfficiencySum += workEfficiency;

                if (hasSkillsComp)
                {
                    var educationValue = GetEducationValue(in skills);
                    educationSkillSum += educationValue;
                    educationSettlers++;
                    if (GetSkillLevelOrZero(in skills, (byte)SettlerSkillId.Research) >= 8)
                        scientistCount++;
                }

                runtime.DaysSinceLastBreak++;

                if (criticalNeedToday > 0 && criticalNeedToday % 6u == 0u)
                    TryEnqueueStoryEvent(ref state, tick, EventNeedCritical, new FixedString64Bytes("settler-need-critical"));
            }

            var birthsToday = SpawnNewSettlersIfNeeded(ref state, day, tick, ref sim, populationAlive);
            populationAlive += birthsToday;
            if (birthsToday > 0)
                TryEnqueueStoryEvent(ref state, tick, EventSettlerBirth, new FixedString64Bytes("settler-birth"));

            sim.PopulationAlive = populationAlive;
            sim.BirthsTotal += birthsToday;
            sim.AverageMood = populationAlive > 0 ? moodSum / populationAlive : 50f;
            sim.AverageStress = populationAlive > 0 ? stressSum / populationAlive : 0f;
            sim.AverageHealth01 = populationAlive > 0 ? health01Sum / populationAlive : 1f;
            sim.AverageWorkEfficiency01 = populationAlive > 0 ? workEfficiencySum / populationAlive : 1f;
            sim.ResourceFoodDemandPerDay = foodDemand;
            sim.ResourceFoodSatisfied01 = foodDemand > 1e-3f ? math.saturate(foodConsumed / foodDemand) : 1f;
            sim.EducationIndex01 = educationSettlers > 0 ? math.saturate((educationSkillSum / educationSettlers) / 20f) : 0f;
            sim.ColonyMorale01 = math.saturate(
                ((sim.AverageMood + 100f) / 200f) * 0.6f +
                (1f - math.saturate(sim.AverageStress / 100f)) * 0.2f +
                sim.AverageHealth01 * 0.2f);

            if (SystemAPI.HasSingleton<ColonyDemographyState>())
            {
                ref var demography = ref SystemAPI.GetSingletonRW<ColonyDemographyState>().ValueRW;
                demography.Population = populationAlive;
                demography.BirthsThisYear += birthsToday;
                demography.DeathsThisYear += deathsToday;
            }

            if (SystemAPI.HasSingleton<CrimeJusticeState>())
            {
                ref var justice = ref SystemAPI.GetSingletonRW<CrimeJusticeState>().ValueRW;
                var stressFactor = math.saturate(sim.AverageStress / 100f);
                var moraleFactor = 1f - sim.ColonyMorale01;
                justice.PovertyPercent = math.clamp(justice.PovertyPercent + moraleFactor * 0.5f - sim.ResourceFoodSatisfied01 * 0.2f,
                    0f, 100f);
                justice.UnemploymentPercent = math.clamp((1f - sim.AverageWorkEfficiency01) * 100f, 0f, 100f);
                justice.OvercrowdingThousands = math.max(0f, colonyOvercrowdingUnits * 0.1f);
                justice.CrimeLevelPercent = math.clamp(justice.CrimeLevelPercent + stressFactor * 1.2f - sim.ColonyMorale01 * 0.6f,
                    0f, 100f);
            }

            if (SystemAPI.HasSingleton<ColonyTechProgressState>())
            {
                ref var tech = ref SystemAPI.GetSingletonRW<ColonyTechProgressState>().ValueRW;
                tech.ScientistsCount = scientistCount;
                tech.ResearchInstitutions = math.max(tech.ResearchInstitutions, math.max(1u, scientistCount / 12u));
            }

            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.SettlerPopulationAlive, sim.PopulationAlive);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.SettlerAverageMood, sim.AverageMood);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.SettlerAverageStress, sim.AverageStress);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.SettlerAverageHealth01, sim.AverageHealth01);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.SettlerAverageWorkEfficiency01,
                sim.AverageWorkEfficiency01);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.SettlerColonyMorale01,
                sim.ColonyMorale01);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.SettlerFoodSatisfied01,
                sim.ResourceFoodSatisfied01);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.SettlerMentalBreaksToday, breaksToday);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.SettlerDeathsToday, deathsToday);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.SettlerBirthsToday, birthsToday);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.SettlerHungryShare01,
                populationAlive > 0 ? (float)hungryToday / populationAlive : 0f);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.SettlerExhaustedShare01,
                populationAlive > 0 ? (float)exhaustedToday / populationAlive : 0f);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.SettlerInfectedShare01,
                populationAlive > 0 ? (float)infectedToday / populationAlive : 0f);
        }

        private static void UpdateNeeds(
            ref NeedsState needs,
            byte taskPriority,
            bool inCombat,
            bool sleeping,
            float foodSatisfied01,
            float waterSatisfied01,
            float housingComfort,
            int overcrowdedUnits)
        {
            var workload = 1f + taskPriority * 0.12f + (inCombat ? 0.6f : 0f);
            needs.Hunger = SettlerSimulationMath.ClampNeed(needs.Hunger + 10f * workload - 18f * foodSatisfied01);
            needs.Thirst = SettlerSimulationMath.ClampNeed(needs.Thirst + 16f * workload - 20f * waterSatisfied01);

            if (sleeping)
                needs.Rest = SettlerSimulationMath.ClampNeed(needs.Rest - 18f);
            else
                needs.Rest = SettlerSimulationMath.ClampNeed(needs.Rest + 8f * workload);

            needs.Recreation = SettlerSimulationMath.ClampNeed(needs.Recreation + 4f * workload - 2f);
            needs.Comfort = SettlerSimulationMath.ClampNeed(needs.Comfort + (60f - housingComfort) * 0.08f);
            needs.Beauty = SettlerSimulationMath.ClampNeed(needs.Beauty + (55f - housingComfort) * 0.06f);
            needs.Space = SettlerSimulationMath.ClampNeed(needs.Space + math.max(0, overcrowdedUnits) * 0.3f);
            needs.TemperatureComfort = SettlerSimulationMath.ClampNeed(needs.TemperatureComfort - 1.5f + housingComfort * 0.01f);
        }

        private static float TickMoodModifiers(ref MoodModifiers mood)
        {
            var total = 0f;
            for (var i = mood.Modifiers.Length - 1; i >= 0; i--)
            {
                var modifier = mood.Modifiers[i];
                total += modifier.Value;
                if (modifier.Duration > 0)
                {
                    if (modifier.Remaining > 0)
                        modifier.Remaining--;
                    if (modifier.Remaining == 0)
                    {
                        mood.Modifiers.RemoveAt(i);
                        continue;
                    }
                    mood.Modifiers[i] = modifier;
                }
            }

            return total;
        }

        private static float UpdateSocialAndRelationships(
            ref SocialBonds social,
            ref SettlerSimulationState sim,
            uint day,
            uint runtimeId,
            ref DynamicBuffer<SettlerRelationshipEdge> edges)
        {
            if (social.Bonds.Length == 0)
                return 0f;

            var valueSum = 0f;
            for (var i = 0; i < social.Bonds.Length; i++)
            {
                var bond = social.Bonds[i];
                bond.Duration = (ushort)math.min(ushort.MaxValue, bond.Duration + 1);
                if (day % 3u == 0)
                {
                    if (bond.Value < 100)
                        bond.Value++;
                    bond.InteractionCount = (byte)math.min(byte.MaxValue, bond.InteractionCount + 1);
                    sim.InteractionEventsTotal++;
                }

                valueSum += bond.Value;
                social.Bonds[i] = bond;

                if (edges.Length < 1024)
                {
                    edges.Add(new SettlerRelationshipEdge
                    {
                        SourceSettlerId = runtimeId,
                        TargetSettlerId = (uint)math.max(0, bond.TargetId),
                        Value = bond.Value,
                        RelationshipType = bond.BondType,
                        DurationDays = bond.Duration
                    });
                }
            }

            return math.saturate((valueSum / social.Bonds.Length + 100f) / 200f);
        }

        private static void UpdatePhysiology(
            ref PhysiologyState physio,
            ref NeedsState needs,
            ref InjuryTracker injuries,
            ref MedicalConditions medical,
            in PersonalityTraits traits,
            in MentalConditions mental,
            float pollutionHealthMultiplier,
            ref Random random)
        {
            var painFromInjuries = 0f;
            var bleedRate = 0f;
            var infectionRisk = 0f;

            for (var i = injuries.Injuries.Length - 1; i >= 0; i--)
            {
                var injury = injuries.Injuries[i];
                painFromInjuries += injury.Severity * 0.06f;
                if (!injury.IsTended)
                    infectionRisk += injury.Severity * 0.35f;
                bleedRate += injury.Severity * 0.02f;
                injury.Age = (ushort)math.min(ushort.MaxValue, injury.Age + 1);

                var healingRate = injury.IsTended ? 0.12f : 0.05f;
                if (SettlerCharacterGenerator.HasTrait(traits, 16))
                    healingRate *= 1.5f;
                if (SettlerCharacterGenerator.HasTrait(traits, 17))
                    healingRate *= 0.7f;
                injury.HealingProgress = math.saturate(injury.HealingProgress + healingRate);
                injury.Severity = math.max(0f, injury.Severity - healingRate * 6f);
                if (injury.HealingProgress >= 0.999f || injury.Severity <= 0.01f)
                {
                    injuries.Injuries.RemoveAt(i);
                    continue;
                }
                injuries.Injuries[i] = injury;
            }

            injuries.InjuryCount = (byte)math.min(byte.MaxValue, injuries.Injuries.Length);
            injuries.BleedingRate = bleedRate;
            injuries.InfectionRisk = infectionRisk;
            injuries.HasCriticalInjury = false;
            for (var i = 0; i < injuries.Injuries.Length; i++)
            {
                if (injuries.Injuries[i].Severity > 80f)
                {
                    injuries.HasCriticalInjury = true;
                    break;
                }
            }

            physio.Pain = math.clamp(painFromInjuries + needs.Hunger * 0.08f + needs.Thirst * 0.08f, 0f, 100f);
            physio.BloodVolume = math.clamp(physio.BloodVolume - injuries.BleedingRate, 0f, 100f);

            var healthDamage = 0f;
            if (needs.Hunger > 70f)
                healthDamage += (needs.Hunger - 70f) * 0.10f;
            if (needs.Thirst > 70f)
                healthDamage += (needs.Thirst - 70f) * 0.12f;
            if (physio.BloodVolume < 50f)
                healthDamage += (50f - physio.BloodVolume) * 0.15f;
            healthDamage += medical.Toxicity * 0.02f;
            healthDamage += medical.Radiation * 0.03f;
            if (pollutionHealthMultiplier < 1f)
                healthDamage += (1f - pollutionHealthMultiplier) * 8f;

            if (injuries.InfectionRisk > 0f && random.NextFloat() < math.saturate(injuries.InfectionRisk / 200f))
            {
                if (medical.Conditions.Length < 6)
                {
                    medical.Conditions.Add(new Condition
                    {
                        ConditionId = 1,
                        Severity = 10,
                        Duration = 10,
                        Progress = 0,
                        IsChronic = false
                    });
                }
            }

            for (var i = 0; i < medical.Conditions.Length; i++)
            {
                var condition = medical.Conditions[i];
                condition.Progress = (ushort)math.min(ushort.MaxValue, condition.Progress + 1);
                if (condition.Severity < 100)
                    condition.Severity++;
                if (condition.Duration > 0)
                    condition.Duration--;
                if (condition.Duration == 0 && !condition.IsChronic)
                {
                    condition.Severity = (byte)math.max(0, condition.Severity - 4);
                }

                medical.Conditions[i] = condition;
                healthDamage += condition.Severity * 0.015f;
            }

            healthDamage += mental.PTSDLevel * 0.005f;
            physio.Health = math.clamp(physio.Health - healthDamage, 0f, physio.MaxHealth);
            if (pollutionHealthMultiplier > 1f)
                physio.Health = math.min(physio.MaxHealth, physio.Health + (pollutionHealthMultiplier - 1f) * 1.2f);

            var health01 = math.saturate(physio.Health / math.max(1f, physio.MaxHealth));
            physio.Consciousness = math.clamp(100f * health01 - physio.Pain * 0.4f, 0f, 100f);
            physio.Mobility = math.clamp(100f * health01 - physio.Pain * 0.5f, 0f, 100f);
            physio.Manipulation = math.clamp(100f * health01 - physio.Pain * 0.35f, 0f, 100f);
            physio.Breathing = math.clamp(100f - medical.Toxicity * 0.8f, 0f, 100f);
            physio.BloodPumping = math.clamp(physio.BloodVolume, 0f, 100f);
        }

        private static bool HasActiveInfection(in MedicalConditions medical)
        {
            for (var i = 0; i < medical.Conditions.Length; i++)
            {
                if (medical.Conditions[i].ConditionId == 1 && medical.Conditions[i].Severity > 15)
                    return true;
            }

            return false;
        }

        private static void UpdateSkillProgress(
            ref SkillSet skills,
            ref SkillUsageTracker usage,
            in Aptitudes aptitudes,
            ref CurrentTask task,
            ref SettlerStats stats,
            in PsychologyState psych,
            uint day,
            ref Random random,
            out float passionBonus01)
        {
            passionBonus01 = 0f;
            while (usage.DaysSinceUse.Length < skills.Skills.Length && usage.DaysSinceUse.Length < 20)
                usage.DaysSinceUse.Add(0);
            while (usage.WastedXp.Length < skills.Skills.Length && usage.WastedXp.Length < 20)
                usage.WastedXp.Add(0);

            for (var i = 0; i < usage.DaysSinceUse.Length; i++)
                usage.DaysSinceUse[i] = (byte)math.min(byte.MaxValue, usage.DaysSinceUse[i] + 1);

            if (SettlerSimulationMath.TryGetTaskSkillId(task.TaskType, out var taskSkill))
            {
                var idx = FindSkillIndex(in skills, taskSkill);
                if (idx >= 0)
                {
                    var skill = skills.Skills[idx];
                    var category = SettlerSimulationMath.GetSkillCategory(skill.SkillId);
                    var aptitudeLevel = SettlerCharacterGenerator.ResolveAptitudeLevel(aptitudes, category);
                    var qualityMultiplier = 0.8f + math.saturate((psych.Mood + 100f) / 200f) * 0.7f;
                    var xpGain = SettlerSimulationMath.ComputeActionXp(
                        1f + task.Priority * 0.3f,
                        qualityMultiplier,
                        skill.PassionLevel,
                        aptitudeLevel,
                        skill.Level);
                    var xpInt = (ushort)math.max(1, (int)math.round(xpGain));
                    if (skill.Level >= skill.NaturalCap && random.NextFloat() > 0.1f)
                    {
                        var wasted = (ushort)math.max(1, (int)math.round(xpInt * 0.9f));
                        var previousWasted = usage.WastedXp[idx];
                        usage.WastedXp[idx] = (ushort)math.min(ushort.MaxValue, previousWasted + wasted);
                        xpInt -= wasted;
                    }

                    skill.Experience = (ushort)math.min(ushort.MaxValue, skill.Experience + xpInt);
                    while (skill.Level < skill.LearnedCap && skill.Experience >= skill.ExperienceToNext)
                    {
                        skill.Experience = (ushort)math.max(0, skill.Experience - skill.ExperienceToNext);
                        skill.Level++;
                        skill.ExperienceToNext = SettlerSimulationMath.ComputeExperienceToNextLevel(skill.Level);
                    }

                    skill.TotalUses = (ushort)math.min(ushort.MaxValue, skill.TotalUses + 1);
                    skills.Skills[idx] = skill;
                    usage.DaysSinceUse[idx] = 0;
                    passionBonus01 = skill.PassionLevel * 0.1f;
                }
            }

            for (var i = 0; i < skills.Skills.Length; i++)
            {
                var skill = skills.Skills[i];
                if (skill.Level == 0 || usage.DaysSinceUse[i] <= 30)
                    continue;

                var decayChance = SettlerSimulationMath.ComputeSkillDecayChance(usage.DaysSinceUse[i]);
                if (random.NextFloat() < decayChance)
                {
                    var decay = SettlerSimulationMath.ComputeSkillDecayAmount(skill.ExperienceToNext);
                    if (skill.Experience > decay)
                    {
                        skill.Experience -= decay;
                    }
                    else if (skill.Level > 0)
                    {
                        skill.Level--;
                        skill.Experience = 0;
                        skill.ExperienceToNext = SettlerSimulationMath.ComputeExperienceToNextLevel(skill.Level);
                    }

                    skills.Skills[i] = skill;
                }
            }

            var researchLevel = GetSkillLevelOrZero(in skills, (byte)SettlerSkillId.Research);
            var medicineLevel = GetSkillLevelOrZero(in skills, (byte)SettlerSkillId.Medicine);
            var growingLevel = GetSkillLevelOrZero(in skills, (byte)SettlerSkillId.Growing);
            if (task.TaskType == 3)
                stats.ResearchPoints += (uint)math.max(1f, math.round(researchLevel * 0.2f));
            if (task.TaskType == 8)
                stats.PatientsTreated += (uint)math.max(0f, math.round(medicineLevel * 0.05f));
            if (task.TaskType == 7)
                stats.CropsHarvested += (uint)math.max(0f, math.round(growingLevel * 0.05f));

            skills.TotalSkillPoints = 0;
            for (var i = 0; i < skills.Skills.Length; i++)
                skills.TotalSkillPoints = (ushort)math.min(ushort.MaxValue, skills.TotalSkillPoints + skills.Skills[i].Level);

            RefreshPrimaryRoles(ref skills);
            task.Priority = (byte)math.clamp(task.Priority, (byte)1, (byte)10);
            _ = day;
        }

        private static void RefreshPrimaryRoles(ref SkillSet skills)
        {
            byte bestRole = 0;
            byte secondRole = 0;
            byte bestLevel = 0;
            byte secondLevel = 0;
            for (byte i = 0; i < skills.Skills.Length; i++)
            {
                var level = skills.Skills[i].Level;
                if (level > bestLevel)
                {
                    secondRole = bestRole;
                    secondLevel = bestLevel;
                    bestRole = i;
                    bestLevel = level;
                }
                else if (level > secondLevel)
                {
                    secondRole = i;
                    secondLevel = level;
                }
            }

            skills.PrimaryRole = bestRole;
            skills.SecondaryRole = secondRole;
        }

        private static int FindSkillIndex(in SkillSet skills, byte skillId)
        {
            for (var i = 0; i < skills.Skills.Length; i++)
            {
                if (skills.Skills[i].SkillId == skillId)
                    return i;
            }

            return -1;
        }

        private static float GetEducationValue(in SkillSet skills)
        {
            var research = GetSkillLevelOrZero(in skills, (byte)SettlerSkillId.Research);
            var medicine = GetSkillLevelOrZero(in skills, (byte)SettlerSkillId.Medicine);
            var programming = GetSkillLevelOrZero(in skills, (byte)SettlerSkillId.Programming);
            var social = GetSkillLevelOrZero(in skills, (byte)SettlerSkillId.Social);
            var leadership = GetSkillLevelOrZero(in skills, (byte)SettlerSkillId.Leadership);
            return (research + medicine + programming + social + leadership) * 0.2f;
        }

        private static byte GetSkillLevelOrZero(in SkillSet skills, byte skillId)
        {
            var idx = FindSkillIndex(in skills, skillId);
            return idx >= 0 ? skills.Skills[idx].Level : (byte)0;
        }

        private static void UpdateAutonomyAndAi(
            ref AutonomyLevel autonomy,
            ref AIState ai,
            in SettlerAutonomyPolicyState policy,
            in CurrentTask task,
            in CommandHierarchy hierarchy,
            in PsychologyState psych,
            in PhysiologyState physio,
            bool drafted,
            bool inCombat,
            uint tick,
            ref Random random)
        {
            var targetLevel = policy.DefaultAutonomyLevel;
            if (policy.GlobalAlertLevel >= 2 || drafted || inCombat)
                targetLevel = (byte)math.max(targetLevel, 3);
            if (policy.GlobalAlertLevel >= 3)
                targetLevel = 4;
            if (physio.Health < policy.SafetyOverrideHealthThreshold || psych.CurrentBreakRisk >= 3)
                targetLevel = (byte)math.min(targetLevel, 1);

            autonomy.PreviousLevel = autonomy.Level;
            if (targetLevel < autonomy.Level)
                autonomy.Level = targetLevel;
            else
                autonomy.Level = targetLevel;

            autonomy.ReactionTime = SettlerSimulationMath.GetAutonomyReactionTime(autonomy.Level) * (1f + psych.Stress / 200f);
            autonomy.AllowedBehaviors = autonomy.Level == 4 ? 0x0000000Fu : uint.MaxValue;

            ai.DecisionCooldown = SettlerSimulationMath.GetDecisionCooldown(autonomy.Level);
            ai.Confidence = math.clamp(
                1f - psych.Stress / 150f + hierarchy.LeadershipBonus * 0.35f + (task.Priority - 1) * 0.01f,
                0f, 1f);
            ai.ConsideredOptions = (uint)math.max(1f, math.round(2f + hierarchy.LeadershipBonus * 20f + 4 - autonomy.Level));
            ai.ChosenOption = (byte)random.NextInt(0, math.max(1, (int)ai.ConsideredOptions));
            ai.LastDecisionTick = (byte)(tick & 0xFF);
        }

        private static float ConsumeFoodRation(ref DynamicBuffer<ResourceStockEntry> stockpile, float required)
        {
            var remaining = required;
            remaining -= ConsumeFromResource(ref stockpile, ResourceId.CropWheat, remaining);
            remaining -= ConsumeFromResource(ref stockpile, ResourceId.CropPotato, remaining);
            remaining -= ConsumeFromResource(ref stockpile, ResourceId.CropVegetables, remaining);
            remaining -= ConsumeFromResource(ref stockpile, ResourceId.CropCorn, remaining);
            remaining -= ConsumeFromResource(ref stockpile, ResourceId.LivestockMeat, remaining);
            remaining -= ConsumeFromResource(ref stockpile, ResourceId.LivestockEggs, remaining);
            remaining -= ConsumeFromResource(ref stockpile, ResourceId.FishCatch, remaining);
            return math.max(0f, required - remaining);
        }

        private static float ConsumeFromResource(ref DynamicBuffer<ResourceStockEntry> stockpile, ResourceId resourceId, float required)
        {
            if (required <= 0f)
                return 0f;

            var available = ResourceStockpileOps.GetAmount(ref stockpile, resourceId);
            if (available <= 0f)
                return 0f;
            var consumed = math.min(available, required);
            ResourceStockpileOps.TryConsume(ref stockpile, resourceId, consumed);
            return consumed;
        }

        private static uint SpawnNewSettlersIfNeeded(
            ref SystemState state,
            uint day,
            uint tick,
            ref SettlerSimulationState sim,
            uint currentPopulation)
        {
            var moraleFactor = math.saturate((sim.ColonyMorale01 - 0.4f) * 1.8f);
            var foodFactor = math.saturate(sim.ResourceFoodSatisfied01);
            var expectedBirths = currentPopulation * 0.002f * moraleFactor * foodFactor;
            var random = Random.CreateFromIndex(math.max(1u, day * 7919u + currentPopulation * 13u));
            var births = (uint)math.floor(expectedBirths);
            if (random.NextFloat() < expectedBirths - births)
                births++;
            births = math.min(2u, births);

            if (currentPopulation < 12 && sim.ColonyMorale01 > 0.55f && random.NextFloat() < 0.4f)
                births = math.max(births, 1u);

            var em = state.EntityManager;
            for (var i = 0u; i < births; i++)
            {
                sim.LastSettlerId++;
                SettlerEntityFactory.CreateSettler(em, sim.LastSettlerId, day, tick + i * 97u, 0, false);
            }

            return births;
        }

        private static void TryEnqueueStoryEvent(ref SystemState state, uint tick, uint eventDefinitionId,
            in FixedString64Bytes label)
        {
            if (!SystemAPI.HasSingleton<StoryEventQueueSingleton>())
                return;
            var queue = SystemAPI.GetSingletonBuffer<GameEventQueueEntry>(ref state);
            queue.Add(new GameEventQueueEntry
            {
                Kind = StoryEventKind.Triggered,
                EventDefinitionId = eventDefinitionId,
                EnqueueSimulationTick = tick,
                DebugLabel = label
            });
        }
    }
}
