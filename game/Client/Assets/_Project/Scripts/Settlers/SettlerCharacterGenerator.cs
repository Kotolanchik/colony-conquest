using Unity.Collections;
using Unity.Mathematics;

namespace ColonyConquest.Settlers
{
    /// <summary>Параметры генерации поселенца.</summary>
    public struct SettlerGenerationParams
    {
        public byte OriginFaction;
        public bool IsVeteran;
    }

    /// <summary>Генератор персонажа по разделу §2 спеки поселенцев.</summary>
    public static class SettlerCharacterGenerator
    {
        public static SettlerIdentity GenerateIdentity(ref Random random, uint settlerId, in SettlerGenerationParams parameters)
        {
            var gender = (byte)random.NextInt(0, 2);
            var age = (byte)random.NextInt(16, 81);
            var nameIdx = random.NextInt(0, 16);
            var surnameIdx = random.NextInt(0, 16);
            var nicknameRoll = random.NextFloat();

            return new SettlerIdentity
            {
                Name = GetFirstName(nameIdx, gender),
                Surname = GetSurname(surnameIdx),
                Nickname = nicknameRoll < 0.2f ? GetNickname(random.NextInt(0, 12)) : default,
                GenerationSeed = (uint)random.NextInt(1, int.MaxValue),
                Gender = gender,
                Age = age,
                OriginFaction = parameters.OriginFaction,
                PortraitId = random.NextInt(0, 128)
            };
        }

        public static SettlerAppearance GenerateAppearance(ref Random random)
        {
            return new SettlerAppearance
            {
                BodyType = (byte)random.NextInt(0, 8),
                SkinTone = (byte)random.NextInt(0, 16),
                HairStyle = (byte)random.NextInt(0, 32),
                HairColor = (byte)random.NextInt(0, 16),
                FacialHair = (byte)random.NextInt(0, 16),
                EyeColor = (byte)random.NextInt(0, 8),
                ScarsBitmask = (ushort)random.NextInt(0, ushort.MaxValue + 1),
                Prosthetics = 0,
                Height = random.NextFloat(1.5f, 2f)
            };
        }

        public static PersonalityTraits GenerateTraits(ref Random random)
        {
            var result = new PersonalityTraits();
            var traitCount = random.NextInt(2, 5);
            uint compatibilityMask = 0u;

            for (var i = 0; i < traitCount && result.ActiveTraits.Length < 8; i++)
            {
                if (!TrySelectWeightedTrait(ref random, in result, compatibilityMask, out var traitId))
                    break;

                var intensity = (sbyte)random.NextInt(-1, 3);
                result.ActiveTraits.Add(new TraitSlot
                {
                    TraitId = traitId,
                    Intensity = intensity,
                    Source = 0
                });

                if (traitId < 32)
                    compatibilityMask |= 1u << traitId;
                var incompatible = GetIncompatibleTrait(traitId);
                if (incompatible < 32)
                    compatibilityMask |= 1u << incompatible;
            }

            result.TraitCompatibilityMask = compatibilityMask;
            return result;
        }

        public static Aptitudes GenerateAptitudes(ref Random random)
        {
            var aptitudes = new Aptitudes();
            var usedCategories = 0u;
            var naturalCount = random.NextInt(1, 4);

            for (var i = 0; i < naturalCount; i++)
            {
                var category = (byte)random.NextInt(0, 5);
                var guard = 0;
                while (((usedCategories >> category) & 1u) != 0 && guard++ < 12)
                    category = (byte)random.NextInt(0, 5);
                usedCategories |= 1u << category;

                aptitudes.NaturalTalents.Add(new Aptitude
                {
                    SkillCategory = category,
                    AptitudeLevel = (byte)random.NextInt(2, 6)
                });
            }

            var acquiredCount = random.NextInt(0, 3);
            for (var i = 0; i < acquiredCount && aptitudes.AcquiredTalents.Length < 5; i++)
            {
                aptitudes.AcquiredTalents.Add(new Aptitude
                {
                    SkillCategory = (byte)random.NextInt(0, 5),
                    AptitudeLevel = (byte)random.NextInt(1, 4)
                });
            }

            var affinityCount = random.NextInt(2, 6);
            for (var i = 0; i < affinityCount && aptitudes.SkillAffinities.Length < 24; i++)
                aptitudes.SkillAffinities.Add((byte)random.NextInt(0, 20));

            return aptitudes;
        }

        public static SkillSet GenerateSkills(
            ref Random random,
            in SettlerIdentity identity,
            in PersonalityTraits traits,
            in Aptitudes aptitudes)
        {
            var set = new SkillSet();
            for (byte i = 0; i < 20; i++)
            {
                var naturalCap = random.NextInt(8, 19);
                if (HasTrait(traits, 14))
                    naturalCap += 2;
                if (HasTrait(traits, 15))
                    naturalCap -= 2;
                naturalCap += CountTalentsInCategory(aptitudes, SettlerSimulationMath.GetSkillCategory(i));

                var skill = new Skill
                {
                    SkillId = i,
                    Level = 0,
                    Experience = 0,
                    ExperienceToNext = SettlerSimulationMath.ComputeExperienceToNextLevel(0),
                    PassionLevel = GeneratePassion(ref random, in aptitudes, i),
                    NaturalCap = (byte)math.clamp(naturalCap, 1, 20),
                    LearnedCap = (byte)math.clamp(naturalCap, 1, 20),
                    TotalUses = 0
                };
                set.Skills.Add(skill);
            }

            var bonusPoints = identity.Age * 2;
            DistributeBonusPoints(ref random, ref set, in aptitudes, bonusPoints);
            set.TotalSkillPoints = ComputeTotalSkillPoints(in set);
            AssignPrimarySecondaryRoles(ref set);
            return set;
        }

        public static MentalConditions GenerateMentalConditions(
            ref Random random,
            in PersonalityTraits traits,
            in SettlerGenerationParams parameters)
        {
            var conditions = new MentalConditions();
            var phobiaChance = 0.15f;
            if (HasTrait(traits, 1))
                phobiaChance += 0.2f;
            if (HasTrait(traits, 15))
                phobiaChance += 0.1f;

            if (random.NextFloat() < phobiaChance)
            {
                conditions.Phobias.Add(new Phobia
                {
                    PhobiaType = (byte)random.NextInt(0, 16),
                    Severity = (byte)random.NextInt(3, 8),
                    TriggerMask = 1u << random.NextInt(0, 16)
                });
            }

            if (parameters.IsVeteran && random.NextFloat() < 0.25f)
                conditions.PTSDLevel = (byte)random.NextInt(10, 50);

            if (HasTrait(traits, 3))
                conditions.DepressionLevel = (byte)random.NextInt(20, 45);
            else
                conditions.DepressionLevel = (byte)random.NextInt(0, 15);

            return conditions;
        }

        public static MoodModifiers BuildMoodModifiers(in PersonalityTraits traits)
        {
            var baseMood = 50f;
            if (HasTrait(traits, 2))
                baseMood += 15f;
            if (HasTrait(traits, 3))
                baseMood -= 15f;
            if (HasTrait(traits, 26))
                baseMood += 5f;
            if (HasTrait(traits, 25))
                baseMood -= 5f;
            return new MoodModifiers
            {
                BaseMood = baseMood
            };
        }

        public static PsychologyState BuildPsychologyState(in MoodModifiers moodModifiers)
        {
            return new PsychologyState
            {
                Mood = moodModifiers.BaseMood,
                MoodTrend = 0f,
                Stress = 5f,
                StressResistance = 20f,
                MentalBreakThreshold = SettlerSimulationMath.ComputeBreakThreshold(moodModifiers.BaseMood, 0),
                MentalBreakRecovery = 1f,
                CurrentBreakRisk = 0,
                ActiveBreakType = 0
            };
        }

        public static PhysiologyState BuildPhysiologyState(ref Random random)
        {
            return new PhysiologyState
            {
                Health = random.NextFloat(75f, 100f),
                MaxHealth = 100f,
                BloodVolume = 100f,
                Pain = random.NextFloat(0f, 5f),
                PainTolerance = random.NextFloat(40f, 70f),
                Consciousness = 100f,
                Mobility = random.NextFloat(75f, 100f),
                Manipulation = random.NextFloat(75f, 100f),
                Vision = random.NextFloat(70f, 100f),
                Hearing = random.NextFloat(70f, 100f),
                Breathing = random.NextFloat(85f, 100f),
                BloodPumping = random.NextFloat(85f, 100f)
            };
        }

        public static NeedsState BuildNeedsState(ref Random random)
        {
            return new NeedsState
            {
                Hunger = random.NextFloat(4f, 16f),
                Thirst = random.NextFloat(4f, 16f),
                Rest = random.NextFloat(8f, 28f),
                Recreation = random.NextFloat(8f, 24f),
                Comfort = random.NextFloat(55f, 85f),
                Beauty = random.NextFloat(40f, 80f),
                Space = random.NextFloat(45f, 85f),
                TemperatureComfort = random.NextFloat(65f, 90f)
            };
        }

        public static InjuryTracker BuildInjuryTracker()
        {
            return new InjuryTracker
            {
                InjuryCount = 0,
                HasCriticalInjury = false,
                BleedingRate = 0f,
                InfectionRisk = 0f
            };
        }

        public static MedicalConditions BuildMedicalConditions(ref Random random)
        {
            return new MedicalConditions
            {
                Immunity = (byte)random.NextInt(45, 86),
                Toxicity = 0f,
                Radiation = 0f
            };
        }

        public static SocialBonds BuildSocialBonds()
        {
            return new SocialBonds
            {
                PartnerId = -1,
                MentorId = -1,
                RivalId = -1
            };
        }

        public static AutonomyLevel BuildAutonomyLevel()
        {
            return new AutonomyLevel
            {
                Level = 1,
                PreviousLevel = 1,
                AllowedBehaviors = uint.MaxValue,
                ReactionTime = SettlerSimulationMath.GetAutonomyReactionTime(1)
            };
        }

        public static CurrentTask BuildCurrentTask(ref Random random)
        {
            var taskType = (byte)random.NextInt(1, 10);
            return new CurrentTask
            {
                TaskType = taskType,
                TargetEntity = -1,
                TargetPosition = float3.zero,
                Priority = (byte)random.NextInt(2, 6),
                TimeLimit = (ushort)random.NextInt(30, 240),
                AssignedBy = 0
            };
        }

        public static AIState BuildAiState()
        {
            return new AIState
            {
                BehaviorState = 0,
                LastDecisionTick = 0,
                DecisionCooldown = SettlerSimulationMath.GetDecisionCooldown(1),
                ConsideredOptions = 0,
                ChosenOption = 0,
                Confidence = 0.8f
            };
        }

        public static CommandHierarchy BuildCommandHierarchy(ref Random random)
        {
            return new CommandHierarchy
            {
                CommanderId = -1,
                SubordinateCount = 0,
                UnitId = random.NextInt(0, 4),
                Rank = (byte)random.NextInt(0, 6),
                CommandStyle = (byte)random.NextInt(0, 5),
                LeadershipBonus = random.NextFloat(0f, 0.12f)
            };
        }

        public static CommanderAI BuildCommanderAi(in CommandHierarchy hierarchy)
        {
            return new CommanderAI
            {
                CommanderId = hierarchy.CommanderId,
                CommandStyle = hierarchy.CommandStyle,
                LeadershipSkill = math.saturate(hierarchy.LeadershipBonus + 0.4f),
                TacticalSkill = 0.4f,
                Aggressiveness = 5,
                CautionLevel = 5,
                CurrentOrderId = -1,
                OrderConfidence = 0.6f
            };
        }

        public static LifecycleState BuildLifecycleState(in SettlerIdentity identity)
        {
            var stage = identity.Age < 18 ? (byte)0 : identity.Age < 60 ? (byte)1 : (byte)2;
            return new LifecycleState
            {
                LifeStage = stage,
                BirthTick = 0,
                DeathTick = -1,
                DeathCause = 0,
                KillerId = -1
            };
        }

        public static SettlerStats BuildStats()
        {
            return default;
        }

        public static SettlerRuntimeState BuildRuntimeState(uint birthDay)
        {
            return new SettlerRuntimeState
            {
                BirthDay = birthDay,
                DaysSinceLastBreak = 0,
                LastSkillDecayDay = birthDay,
                WorkContributionToday = 0f
            };
        }

        public static SkillUsageTracker BuildSkillUsageTracker()
        {
            var tracker = new SkillUsageTracker();
            for (var i = 0; i < 20; i++)
            {
                tracker.DaysSinceUse.Add(0);
                tracker.WastedXp.Add(0);
            }
            return tracker;
        }

        public static void ApplyTraitModifiers(
            in PersonalityTraits traits,
            ref PsychologyState psych,
            ref PhysiologyState physio,
            ref NeedsState needs)
        {
            if (HasTrait(traits, 0))
                psych.StressResistance += 15f;
            if (HasTrait(traits, 1))
                psych.StressResistance -= 15f;
            if (HasTrait(traits, 31))
                physio.PainTolerance += 12f;
            if (HasTrait(traits, 32))
                physio.PainTolerance -= 12f;
            if (HasTrait(traits, 16))
            {
                physio.MaxHealth += 10f;
                physio.Health += 5f;
            }
            if (HasTrait(traits, 17))
            {
                physio.MaxHealth -= 20f;
                physio.Health -= 10f;
            }
            if (HasTrait(traits, 10))
                needs.Comfort = math.max(0f, needs.Comfort - 15f);
            if (HasTrait(traits, 11))
                needs.Comfort = math.min(100f, needs.Comfort + 15f);

            physio.MaxHealth = math.clamp(physio.MaxHealth, 30f, 120f);
            physio.Health = math.clamp(physio.Health, 1f, physio.MaxHealth);
            psych.StressResistance = math.clamp(psych.StressResistance, 0f, 100f);
            physio.PainTolerance = math.clamp(physio.PainTolerance, 0f, 100f);
        }

        public static byte ResolveAptitudeLevel(in Aptitudes aptitudes, byte skillCategory)
        {
            byte level = 0;
            for (var i = 0; i < aptitudes.NaturalTalents.Length; i++)
            {
                var talent = aptitudes.NaturalTalents[i];
                if (talent.SkillCategory == skillCategory)
                    level = math.max(level, talent.AptitudeLevel);
            }
            for (var i = 0; i < aptitudes.AcquiredTalents.Length; i++)
            {
                var talent = aptitudes.AcquiredTalents[i];
                if (talent.SkillCategory == skillCategory)
                    level = math.max(level, talent.AptitudeLevel);
            }
            return level;
        }

        public static bool HasTrait(in PersonalityTraits traits, byte traitId)
        {
            for (var i = 0; i < traits.ActiveTraits.Length; i++)
            {
                if (traits.ActiveTraits[i].TraitId == traitId)
                    return true;
            }
            return false;
        }

        private static bool TrySelectWeightedTrait(
            ref Random random,
            in PersonalityTraits traits,
            uint compatibilityMask,
            out byte traitId)
        {
            var totalWeight = 0;
            for (byte id = 0; id < 40; id++)
            {
                if (HasTrait(traits, id))
                    continue;
                if (id < 32 && ((compatibilityMask >> id) & 1u) != 0)
                    continue;
                totalWeight += GetTraitWeight(id);
            }

            if (totalWeight <= 0)
            {
                traitId = 0;
                return false;
            }

            var roll = random.NextInt(0, totalWeight);
            var acc = 0;
            for (byte id = 0; id < 40; id++)
            {
                if (HasTrait(traits, id))
                    continue;
                if (id < 32 && ((compatibilityMask >> id) & 1u) != 0)
                    continue;
                acc += GetTraitWeight(id);
                if (roll < acc)
                {
                    traitId = id;
                    return true;
                }
            }

            traitId = 0;
            return false;
        }

        private static int GetTraitWeight(byte traitId)
        {
            return traitId switch
            {
                2 => 10,
                3 => 8,
                0 => 8,
                1 => 6,
                4 => 7,
                5 => 8,
                7 => 6,
                10 => 5,
                11 => 6,
                14 => 3,
                15 => 3,
                16 => 5,
                17 => 4,
                20 => 3,
                21 => 4,
                22 => 5,
                _ => 5
            };
        }

        private static byte GetIncompatibleTrait(byte traitId)
        {
            return traitId switch
            {
                0 => 1,
                1 => 0,
                2 => 3,
                3 => 2,
                4 => 5,
                5 => 4,
                6 => 7,
                7 => 6,
                8 => 22,
                22 => 8,
                10 => 11,
                11 => 10,
                12 => 13,
                13 => 12,
                14 => 15,
                15 => 14,
                16 => 17,
                17 => 16,
                18 => 19,
                19 => 18,
                20 => 34,
                21 => 22,
                23 => 24,
                24 => 23,
                25 => 26,
                26 => 25,
                27 => 28,
                28 => 27,
                29 => 30,
                30 => 29,
                31 => 32,
                32 => 31,
                33 => 34,
                34 => 33,
                35 => 36,
                36 => 35,
                38 => 39,
                39 => 38,
                _ => byte.MaxValue
            };
        }

        private static byte GeneratePassion(ref Random random, in Aptitudes aptitudes, byte skillId)
        {
            var baseRoll = random.NextFloat();
            var passion = baseRoll < 0.45f ? 0 : baseRoll < 0.75f ? 1 : baseRoll < 0.93f ? 2 : 3;
            for (var i = 0; i < aptitudes.SkillAffinities.Length; i++)
            {
                if (aptitudes.SkillAffinities[i] != skillId)
                    continue;
                passion = math.min(3, passion + 1);
                break;
            }
            return (byte)passion;
        }

        private static int CountTalentsInCategory(in Aptitudes aptitudes, byte category)
        {
            var count = 0;
            for (var i = 0; i < aptitudes.NaturalTalents.Length; i++)
                if (aptitudes.NaturalTalents[i].SkillCategory == category)
                    count++;
            for (var i = 0; i < aptitudes.AcquiredTalents.Length; i++)
                if (aptitudes.AcquiredTalents[i].SkillCategory == category)
                    count++;
            return count;
        }

        private static void DistributeBonusPoints(ref Random random, ref SkillSet set, in Aptitudes aptitudes, int points)
        {
            var remaining = points;
            while (remaining > 0)
            {
                var index = random.NextInt(0, set.Skills.Length);
                var skill = set.Skills[index];
                var category = SettlerSimulationMath.GetSkillCategory(skill.SkillId);
                var aptitude = ResolveAptitudeLevel(aptitudes, category);
                var gain = (ushort)math.min(remaining, random.NextInt(8, 24) + aptitude * 3);
                skill.Experience = (ushort)math.min(ushort.MaxValue, skill.Experience + gain);
                remaining -= gain;

                while (skill.Level < 20 && skill.Experience >= skill.ExperienceToNext)
                {
                    var overflow = skill.Experience - skill.ExperienceToNext;
                    skill.Level++;
                    skill.Experience = overflow;
                    skill.ExperienceToNext = SettlerSimulationMath.ComputeExperienceToNextLevel(skill.Level);
                }

                set.Skills[index] = skill;
            }
        }

        private static ushort ComputeTotalSkillPoints(in SkillSet set)
        {
            ushort total = 0;
            for (var i = 0; i < set.Skills.Length; i++)
                total = (ushort)math.min(ushort.MaxValue, total + set.Skills[i].Level);
            return total;
        }

        private static void AssignPrimarySecondaryRoles(ref SkillSet set)
        {
            byte bestRole = 0;
            byte secondRole = 0;
            byte bestLevel = 0;
            byte secondLevel = 0;
            for (byte i = 0; i < set.Skills.Length; i++)
            {
                var level = set.Skills[i].Level;
                if (level > bestLevel)
                {
                    secondLevel = bestLevel;
                    secondRole = bestRole;
                    bestLevel = level;
                    bestRole = i;
                }
                else if (level > secondLevel)
                {
                    secondLevel = level;
                    secondRole = i;
                }
            }

            set.PrimaryRole = bestRole;
            set.SecondaryRole = secondRole;
        }

        private static FixedString64Bytes GetFirstName(int index, byte gender)
        {
            if (gender == 1)
            {
                return index switch
                {
                    0 => new FixedString64Bytes("Anna"),
                    1 => new FixedString64Bytes("Mira"),
                    2 => new FixedString64Bytes("Nadia"),
                    3 => new FixedString64Bytes("Lina"),
                    4 => new FixedString64Bytes("Sera"),
                    5 => new FixedString64Bytes("Kira"),
                    6 => new FixedString64Bytes("Yuna"),
                    7 => new FixedString64Bytes("Mila"),
                    8 => new FixedString64Bytes("Vera"),
                    9 => new FixedString64Bytes("Sofia"),
                    10 => new FixedString64Bytes("Elena"),
                    11 => new FixedString64Bytes("Ira"),
                    12 => new FixedString64Bytes("Aya"),
                    13 => new FixedString64Bytes("Nova"),
                    14 => new FixedString64Bytes("Rina"),
                    _ => new FixedString64Bytes("Tala")
                };
            }

            return index switch
            {
                0 => new FixedString64Bytes("Alex"),
                1 => new FixedString64Bytes("Ivan"),
                2 => new FixedString64Bytes("Mark"),
                3 => new FixedString64Bytes("Niko"),
                4 => new FixedString64Bytes("Darin"),
                5 => new FixedString64Bytes("Leo"),
                6 => new FixedString64Bytes("Vlad"),
                7 => new FixedString64Bytes("Oleg"),
                8 => new FixedString64Bytes("Yuri"),
                9 => new FixedString64Bytes("Aron"),
                10 => new FixedString64Bytes("Maks"),
                11 => new FixedString64Bytes("Pavel"),
                12 => new FixedString64Bytes("Rost"),
                13 => new FixedString64Bytes("Tomas"),
                14 => new FixedString64Bytes("Joren"),
                _ => new FixedString64Bytes("Kiril")
            };
        }

        private static FixedString64Bytes GetSurname(int index)
        {
            return index switch
            {
                0 => new FixedString64Bytes("Stone"),
                1 => new FixedString64Bytes("Rivers"),
                2 => new FixedString64Bytes("Volkov"),
                3 => new FixedString64Bytes("Dawn"),
                4 => new FixedString64Bytes("Iron"),
                5 => new FixedString64Bytes("Belov"),
                6 => new FixedString64Bytes("Kane"),
                7 => new FixedString64Bytes("Voss"),
                8 => new FixedString64Bytes("Kovacs"),
                9 => new FixedString64Bytes("Morrow"),
                10 => new FixedString64Bytes("Hale"),
                11 => new FixedString64Bytes("North"),
                12 => new FixedString64Bytes("Sable"),
                13 => new FixedString64Bytes("Rook"),
                14 => new FixedString64Bytes("Rowan"),
                _ => new FixedString64Bytes("Drake")
            };
        }

        private static FixedString64Bytes GetNickname(int index)
        {
            return index switch
            {
                0 => new FixedString64Bytes("Falcon"),
                1 => new FixedString64Bytes("Brick"),
                2 => new FixedString64Bytes("Quiet"),
                3 => new FixedString64Bytes("Viper"),
                4 => new FixedString64Bytes("Ironhand"),
                5 => new FixedString64Bytes("Torch"),
                6 => new FixedString64Bytes("Stoneeye"),
                7 => new FixedString64Bytes("Mender"),
                8 => new FixedString64Bytes("Scribe"),
                9 => new FixedString64Bytes("Ash"),
                10 => new FixedString64Bytes("Rook"),
                _ => new FixedString64Bytes("Warden")
            };
        }
    }
}
