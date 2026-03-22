using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Settlers
{
    /// <summary>Создание сущности поселенца с полным набором ECS-компонентов.</summary>
    public static class SettlerEntityFactory
    {
        public static Entity CreateSettler(
            EntityManager entityManager,
            uint settlerId,
            uint gameDay,
            uint seed,
            byte originFaction,
            bool isVeteran)
        {
            var random = Random.CreateFromIndex(math.max(1u, seed ^ (settlerId * 2654435761u)));
            var generationParams = new SettlerGenerationParams
            {
                OriginFaction = originFaction,
                IsVeteran = isVeteran
            };

            var identity = SettlerCharacterGenerator.GenerateIdentity(ref random, settlerId, generationParams);
            var appearance = SettlerCharacterGenerator.GenerateAppearance(ref random);
            var traits = SettlerCharacterGenerator.GenerateTraits(ref random);
            var aptitudes = SettlerCharacterGenerator.GenerateAptitudes(ref random);
            var skills = SettlerCharacterGenerator.GenerateSkills(ref random, identity, traits, aptitudes);
            var mental = SettlerCharacterGenerator.GenerateMentalConditions(ref random, traits, generationParams);
            var moodModifiers = SettlerCharacterGenerator.BuildMoodModifiers(traits);
            var psychology = SettlerCharacterGenerator.BuildPsychologyState(moodModifiers);
            var physiology = SettlerCharacterGenerator.BuildPhysiologyState(ref random);
            var needs = SettlerCharacterGenerator.BuildNeedsState(ref random);
            SettlerCharacterGenerator.ApplyTraitModifiers(traits, ref psychology, ref physiology, ref needs);
            psychology.MentalBreakThreshold = SettlerSimulationMath.ComputeBreakThreshold(psychology.Mood, mental.PTSDLevel);

            var entity = entityManager.CreateEntity();
            entityManager.AddComponent(entity, identity);
            entityManager.AddComponent(entity, appearance);
            entityManager.AddComponent(entity, traits);
            entityManager.AddComponent(entity, aptitudes);
            entityManager.AddComponent(entity, skills);
            entityManager.AddComponent(entity, mental);
            entityManager.AddComponent(entity, moodModifiers);
            entityManager.AddComponent(entity, SettlerCharacterGenerator.BuildSocialBonds());
            entityManager.AddComponent(entity, psychology);
            entityManager.AddComponent(entity, physiology);
            entityManager.AddComponent(entity, needs);
            entityManager.AddComponent(entity, SettlerCharacterGenerator.BuildInjuryTracker());
            entityManager.AddComponent(entity, SettlerCharacterGenerator.BuildMedicalConditions(ref random));
            entityManager.AddComponent(entity, SettlerCharacterGenerator.BuildAutonomyLevel());
            entityManager.AddComponent(entity, SettlerCharacterGenerator.BuildCurrentTask(ref random));
            entityManager.AddComponent(entity, SettlerCharacterGenerator.BuildAiState());
            var command = SettlerCharacterGenerator.BuildCommandHierarchy(ref random);
            entityManager.AddComponent(entity, command);
            entityManager.AddComponent(entity, SettlerCharacterGenerator.BuildCommanderAi(command));
            var lifecycle = SettlerCharacterGenerator.BuildLifecycleState(identity);
            lifecycle.BirthTick = (int)gameDay;
            entityManager.AddComponent(entity, lifecycle);
            entityManager.AddComponent(entity, SettlerCharacterGenerator.BuildStats());
            entityManager.AddComponent(entity, new SettlerRuntimeId { Value = settlerId });
            entityManager.AddComponent(entity, SettlerCharacterGenerator.BuildRuntimeState(gameDay));
            entityManager.AddComponent(entity, SettlerCharacterGenerator.BuildSkillUsageTracker());

            EnsureStatusTags(entityManager, entity);
            return entity;
        }

        private static void EnsureStatusTags(EntityManager entityManager, Entity entity)
        {
            entityManager.AddComponent<IsIncapacitated>(entity);
            entityManager.SetComponentEnabled<IsIncapacitated>(entity, false);

            entityManager.AddComponent<IsDead>(entity);
            entityManager.SetComponentEnabled<IsDead>(entity, false);

            entityManager.AddComponent<IsSleeping>(entity);
            entityManager.SetComponentEnabled<IsSleeping>(entity, false);

            entityManager.AddComponent<IsInCombat>(entity);
            entityManager.SetComponentEnabled<IsInCombat>(entity, false);

            entityManager.AddComponent<IsDrafted>(entity);
            entityManager.SetComponentEnabled<IsDrafted>(entity, false);

            entityManager.AddComponent<IsCaravanMember>(entity);
            entityManager.SetComponentEnabled<IsCaravanMember>(entity, false);

            entityManager.AddComponent<IsPrisoner>(entity);
            entityManager.SetComponentEnabled<IsPrisoner>(entity, false);

            entityManager.AddComponent<IsSlave>(entity);
            entityManager.SetComponentEnabled<IsSlave>(entity, false);

            entityManager.AddComponent<HasMentalBreak>(entity);
            entityManager.SetComponentEnabled<HasMentalBreak>(entity, false);

            entityManager.AddComponent<IsWounded>(entity);
            entityManager.SetComponentEnabled<IsWounded>(entity, false);

            entityManager.AddComponent<IsInfected>(entity);
            entityManager.SetComponentEnabled<IsInfected>(entity, false);

            entityManager.AddComponent<IsHungry>(entity);
            entityManager.SetComponentEnabled<IsHungry>(entity, false);

            entityManager.AddComponent<IsExhausted>(entity);
            entityManager.SetComponentEnabled<IsExhausted>(entity, false);
        }
    }
}
