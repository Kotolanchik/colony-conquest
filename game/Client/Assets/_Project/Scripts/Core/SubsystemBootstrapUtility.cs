using ColonyConquest.Agriculture;
using ColonyConquest.Analytics;
using ColonyConquest.Audio;
using ColonyConquest.Ecology;
using ColonyConquest.Economy;
using ColonyConquest.Netcode;
using ColonyConquest.Simulation;
using ColonyConquest.Story;
using ColonyConquest.Technology;
using ColonyConquest.WorldMap;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ColonyConquest.Core
{
    /// <summary>Создаёт синглтоны подсистем (аналитика, аудио-очередь, события, карта, стройка, netcode).</summary>
    internal static class SubsystemBootstrapUtility
    {
        internal static void EnsureSubsystemEntities(ref SystemState state)
        {
            var em = state.EntityManager;
            var needFullInit = false;
            using (var check = em.CreateEntityQuery(ComponentType.ReadOnly<AnalyticsServiceSingleton>()))
            {
                if (check.CalculateEntityCount() == 0)
                    needFullInit = true;
            }

            if (needFullInit)
            {
                var analytics = em.CreateEntity();
                em.AddComponent<AnalyticsServiceSingleton>(analytics);
                em.AddBuffer<AnalyticsRecordEntry>(analytics);
                em.AddComponent(analytics, default(AnalyticsLocalSnapshot));

                var audio = em.CreateEntity();
                em.AddComponent<AudioBusServiceSingleton>(audio);
                em.AddBuffer<AudioBusPendingEntry>(audio);

                var story = em.CreateEntity();
                em.AddComponent<StoryEventQueueSingleton>(story);
                em.AddComponent(story, new StoryEventPipelineState());
                em.AddBuffer<GameEventQueueEntry>(story);

                em.CreateSingleton(new WorldMapFocusState
                {
                    PlayerChunk = new MapChunkCoord(0, 0),
                    PreviewBiome = WorldBiomeId.MixedForest,
                    ActiveScale = WorldMapScaleLevel.Local
                });

                em.CreateSingleton(new GameCalendarState());

                em.CreateSingleton(new NetcodeSpikeState { TransportReady = 0 });

                em.CreateSingleton(new ColonyAgrochemicalLoadState { ChemicalLoad01 = 0f });

                var ecoInitial = new ColonyEcologyIndicatorsState
                {
                    AirQuality01 = 0.88f,
                    WaterQuality01 = 0.88f,
                    SoilFertilityIndicator01 = 0.88f,
                    ForestCover01 = 0.88f,
                    Biodiversity01 = 0.88f,
                    LastAgrochemicalBridgeDayIndex = uint.MaxValue
                };
                em.CreateSingleton(ecoInitial);
                var pollution0 = EcologyPollutionMath.GetCombinedPollutionPercent0to100(ecoInitial);
                em.CreateSingleton(new ColonyPollutionSummaryState
                {
                    CombinedPollutionPercent0to100 = pollution0,
                    Band = EcologyPollutionMath.GetPollutionLevelBand(pollution0)
                });

                em.CreateSingleton(new AiDirectorDimensionsState
                {
                    Wealth0to100 = 50f,
                    Security0to100 = 50f,
                    Stability0to100 = 50f,
                    Progress0to100 = 50f,
                    Tension0to100 = 50f
                });

                em.CreateSingleton(new AiDirectorPolicyState
                {
                    ActivePolicy = AiDirectorPolicyKind.None,
                    LastChangeTick = 0
                });

                var stockpile = em.CreateEntity();
                em.AddComponent<ResourceStockpileSingleton>(stockpile);
                var stockBuf = em.AddBuffer<ResourceStockEntry>(stockpile);
                ResourceStockpileOps.Add(ref stockBuf, ResourceId.IronOre, 24f);
                ResourceStockpileOps.Add(ref stockBuf, ResourceId.CopperOre, 24f);
                ResourceStockpileOps.Add(ref stockBuf, ResourceId.CopperIngot, 12f);
                ResourceStockpileOps.Add(ref stockBuf, ResourceId.TinOre, 16f);
                ResourceStockpileOps.Add(ref stockBuf, ResourceId.GoldOre, 8f);
                ResourceStockpileOps.Add(ref stockBuf, ResourceId.SilverOre, 12f);
                ResourceStockpileOps.Add(ref stockBuf, ResourceId.Coal, 48f);
                ResourceStockpileOps.Add(ref stockBuf, ResourceId.Wood, 36f);
                ResourceStockpileOps.Add(ref stockBuf, ResourceId.Stone, 24f);
                ResourceStockpileOps.Add(ref stockBuf, ResourceId.Sulfur, 12f);
                ResourceStockpileOps.Add(ref stockBuf, ResourceId.Saltpeter, 12f);
                ResourceStockpileOps.Add(ref stockBuf, ResourceId.CropWheat, 24f);
                ResourceStockpileOps.Add(ref stockBuf, ResourceId.LivestockMeat, 16f);
                ResourceStockpileOps.Add(ref stockBuf, ResourceId.Oil, 24f);
                ResourceStockpileOps.Add(ref stockBuf, ResourceId.CoalCoke, 12f);
                ResourceStockpileOps.Add(ref stockBuf, ResourceId.SteelRolledPlate, 8f);
                ResourceStockpileOps.Add(ref stockBuf, ResourceId.Planks, 12f);
                ResourceStockpileOps.Add(ref stockBuf, ResourceId.Cloth, 8f);
                ResourceStockpileOps.Add(ref stockBuf, ResourceId.Epoch1Tools, 12f);
                ResourceStockpileOps.Add(ref stockBuf, ResourceId.SteelIndustrial, 24f);
                ResourceStockpileOps.Add(ref stockBuf, ResourceId.TungstenOre, 10f);
                ResourceStockpileOps.Add(ref stockBuf, ResourceId.ChromiteOre, 10f);
                ResourceStockpileOps.Add(ref stockBuf, ResourceId.NickelOre, 10f);
                ResourceStockpileOps.Add(ref stockBuf, ResourceId.Dynamite, 12f);
                ResourceStockpileOps.Add(ref stockBuf, ResourceId.ChemicalReagents, 16f);
                ResourceStockpileOps.Add(ref stockBuf, ResourceId.UraniumOre, 12f);

                em.CreateSingleton(new EconomyWorkshopRuntime
                {
                    ActiveRecipe = ProductionRecipeId.None,
                    Progress01 = 0f,
                    AssignedWorkers = 4,
                    EnergyRatio01 = 1f,
                    ToolCondition01 = 1f,
                    AverageSkill0To100 = 50f,
                    BuildingWear01 = 0f
                });

                var demoMine = em.CreateEntity();
                em.AddComponent(demoMine, new MiningDepositRuntime
                {
                    Kind = MiningDepositKindId.Forest,
                    AmountRemaining = 500f,
                    InitialAmount = 500f
                });
                em.AddComponent(demoMine, LocalTransform.FromPosition(new float3(3f, 0f, 0f)));

                var demoIron = em.CreateEntity();
                em.AddComponent(demoIron, new MiningDepositRuntime
                {
                    Kind = MiningDepositKindId.IronOre,
                    AmountRemaining = 1000f,
                    InitialAmount = 1000f
                });
                em.AddComponent(demoIron, LocalTransform.FromPosition(new float3(6f, 0f, 2f)));

                var demoSilver = em.CreateEntity();
                em.AddComponent(demoSilver, new MiningDepositRuntime
                {
                    Kind = MiningDepositKindId.SilverOre,
                    AmountRemaining = 500f,
                    InitialAmount = 500f
                });
                em.AddComponent(demoSilver, LocalTransform.FromPosition(new float3(8f, 0f, 4f)));

                var demoClay = em.CreateEntity();
                em.AddComponent(demoClay, new MiningDepositRuntime
                {
                    Kind = MiningDepositKindId.Clay,
                    AmountRemaining = 400f,
                    InitialAmount = 400f
                });
                em.AddComponent(demoClay, LocalTransform.FromPosition(new float3(-4f, 0f, 2f)));

                var demoSand = em.CreateEntity();
                em.AddComponent(demoSand, new MiningDepositRuntime
                {
                    Kind = MiningDepositKindId.Sand,
                    AmountRemaining = 400f,
                    InitialAmount = 400f
                });
                em.AddComponent(demoSand, LocalTransform.FromPosition(new float3(-4f, 0f, -2f)));

                var demoStoneQuarry = em.CreateEntity();
                em.AddComponent(demoStoneQuarry, new MiningDepositRuntime
                {
                    Kind = MiningDepositKindId.StoneQuarry,
                    AmountRemaining = 800f,
                    InitialAmount = 800f
                });
                em.AddComponent(demoStoneQuarry, LocalTransform.FromPosition(new float3(9f, 0f, -2f)));

                var fishGather = em.CreateEntity();
                em.AddComponent<WildGatherSpotTag>(fishGather);
                em.AddComponent(fishGather, new WildGatherSpotRuntime { Kind = WildGatherKindId.Fish });
                em.AddComponent(fishGather, LocalTransform.FromPosition(new float3(0f, 0f, -6f)));

                var wildGather = em.CreateEntity();
                em.AddComponent<WildGatherSpotTag>(wildGather);
                em.AddComponent(wildGather, new WildGatherSpotRuntime { Kind = WildGatherKindId.WildGame });
                em.AddComponent(wildGather, LocalTransform.FromPosition(new float3(-9f, 0f, 0f)));

                em.CreateSingleton(new MiningWorldRegenerationState { LastProcessedGameYear = 0 });

                em.CreateSingleton(new WildRenewableStockState
                {
                    FishBiomass = 1000f,
                    FishBiomassCap = 1000f,
                    WildGameBiomass = 500f,
                    WildGameCap = 500f
                });

                var demoQuarry = em.CreateEntity();
                em.AddComponent<IndustrialMiningSiteTag>(demoQuarry);
                em.AddComponent(demoQuarry, new IndustrialMiningSiteRuntime
                {
                    Method = IndustrialMiningMethodId.OpenQuarry,
                    WorkersAssigned = 5,
                    OutputAccumulator = 0f,
                    OutputResourceId = ResourceId.Stone
                });

                var demoPlot = em.CreateEntity();
                em.AddComponent<CropPlotTag>(demoPlot);
                em.AddComponent(demoPlot, new CropPlotRuntime
                {
                    Crop = CropKindId.Wheat,
                    Phase = CropGrowthPhase.Preparation,
                    PhaseStartTick = 0,
                    SoilFertility = 1f,
                    ActiveFertilizer = FertilizerKindId.Manure,
                    PestDamage = 0.05f,
                    WeedPressure01 = 0.08f,
                    FarmerSkillLevel = 2f,
                    WeatherModifier = 1f,
                    WaterSupply = WaterSupplyKind.Normal,
                    LastCareGameDayIndex = uint.MaxValue,
                    LastSoilAnnualGameYearIndex = 0
                });

                var demoPen = em.CreateEntity();
                em.AddComponent<LivestockPenTag>(demoPen);
                em.AddComponent(demoPen, new LivestockPenRuntime
                {
                    Kind = LivestockKindId.Chickens,
                    LastYieldDayIndex = uint.MaxValue
                });

                var demoPenGoats = em.CreateEntity();
                em.AddComponent<LivestockPenTag>(demoPenGoats);
                em.AddComponent(demoPenGoats, new LivestockPenRuntime
                {
                    Kind = LivestockKindId.Goats,
                    LastYieldDayIndex = uint.MaxValue
                });

                var demoPenSheep = em.CreateEntity();
                em.AddComponent<LivestockPenTag>(demoPenSheep);
                em.AddComponent(demoPenSheep, new LivestockPenRuntime
                {
                    Kind = LivestockKindId.Sheep,
                    LastYieldDayIndex = uint.MaxValue
                });

                var demoPenPigs = em.CreateEntity();
                em.AddComponent<LivestockPenTag>(demoPenPigs);
                em.AddComponent(demoPenPigs, new LivestockPenRuntime
                {
                    Kind = LivestockKindId.Pigs,
                    LastYieldDayIndex = uint.MaxValue
                });

            }

            EnsureAnalyticsAndSimulationSingletons(ref em);
        }

        /// <summary>Добавляет новые синглтоны к существующим мирам (миграция после обновления кода).</summary>
        static void EnsureAnalyticsAndSimulationSingletons(ref EntityManager em)
        {
            using (var q = em.CreateEntityQuery(ComponentType.ReadOnly<AnalyticsServiceSingleton>()))
            {
                if (q.CalculateEntityCount() != 0)
                {
                    var e = q.GetSingletonEntity();
                    if (!em.HasComponent<AnalyticsLocalSnapshot>(e))
                        em.AddComponent(e, default(AnalyticsLocalSnapshot));
                }
            }

            using (var q = em.CreateEntityQuery(ComponentType.ReadOnly<ColonyDemographyState>()))
            {
                if (q.CalculateEntityCount() == 0)
                {
                    em.CreateSingleton(new ColonyDemographyState
                    {
                        Population = 100,
                        BirthsThisYear = 0,
                        DeathsThisYear = 0,
                        LastProcessedYearIndex = 0
                    });
                }
            }

            using (var q = em.CreateEntityQuery(ComponentType.ReadOnly<ColonyTechProgressState>()))
            {
                if (q.CalculateEntityCount() == 0)
                {
                    em.CreateSingleton(new ColonyTechProgressState
                    {
                        ResearchPointsPerDay = 10f,
                        TechnologiesUnlocked = 0,
                        CurrentEraProgress01 = 0.1f,
                        ResearchInstitutions = 0,
                        ScientistsCount = 0,
                        CurrentEra = TechEraId.Era1_Foundation,
                        ResearchPointsAccumulated = 0f,
                        LastResearchDayIndex = 0
                    });
                }
            }

            using (var q = em.CreateEntityQuery(ComponentType.ReadOnly<ManualMiningToolState>()))
            {
                if (q.CalculateEntityCount() == 0)
                {
                    em.CreateSingleton(new ManualMiningToolState
                    {
                        Tier = MiningPickaxeTierId.Iron,
                        DurabilityRemaining = MiningManualFormulas.GetMaxDurability(MiningPickaxeTierId.Iron),
                        MinerSkillLevel = 0,
                        SessionWorkHours = 0f
                    });
                }
            }

            using (var q = em.CreateEntityQuery(ComponentType.ReadOnly<MiningHazardProcessState>()))
            {
                if (q.CalculateEntityCount() == 0)
                    em.CreateSingleton(new MiningHazardProcessState { LastProcessedGameDay = uint.MaxValue });
            }

            using (var q = em.CreateEntityQuery(ComponentType.ReadOnly<MiningWorldRegenerationState>()))
            {
                if (q.CalculateEntityCount() == 0)
                    em.CreateSingleton(new MiningWorldRegenerationState { LastProcessedGameYear = 0 });
            }

            using (var q = em.CreateEntityQuery(ComponentType.ReadOnly<WildRenewableStockState>()))
            {
                if (q.CalculateEntityCount() == 0)
                {
                    em.CreateSingleton(new WildRenewableStockState
                    {
                        FishBiomass = 1000f,
                        FishBiomassCap = 1000f,
                        WildGameBiomass = 500f,
                        WildGameCap = 500f
                    });
                }
            }

            using (var q = em.CreateEntityQuery(ComponentType.ReadOnly<EconomyWorkshopRuntime>()))
            {
                if (q.CalculateEntityCount() == 0)
                {
                    em.CreateSingleton(new EconomyWorkshopRuntime
                    {
                        ActiveRecipe = ProductionRecipeId.None,
                        Progress01 = 0f,
                        AssignedWorkers = 4,
                        EnergyRatio01 = 1f,
                        ToolCondition01 = 1f,
                        AverageSkill0To100 = 50f,
                        BuildingWear01 = 0f
                    });
                }
            }

            using (var q = em.CreateEntityQuery(ComponentType.ReadOnly<ColonyAgrochemicalLoadState>()))
            {
                if (q.CalculateEntityCount() == 0)
                    em.CreateSingleton(new ColonyAgrochemicalLoadState { ChemicalLoad01 = 0f });
            }

            using (var q = em.CreateEntityQuery(ComponentType.ReadOnly<ColonyEcologyIndicatorsState>()))
            {
                if (q.CalculateEntityCount() == 0)
                {
                    var ecoInitial = new ColonyEcologyIndicatorsState
                    {
                        AirQuality01 = 0.88f,
                        WaterQuality01 = 0.88f,
                        SoilFertilityIndicator01 = 0.88f,
                        ForestCover01 = 0.88f,
                        Biodiversity01 = 0.88f,
                        LastAgrochemicalBridgeDayIndex = uint.MaxValue
                    };
                    em.CreateSingleton(ecoInitial);
                }
            }

            using (var qPol = em.CreateEntityQuery(ComponentType.ReadOnly<ColonyPollutionSummaryState>()))
            using (var qEco = em.CreateEntityQuery(ComponentType.ReadOnly<ColonyEcologyIndicatorsState>()))
            {
                if (qPol.CalculateEntityCount() == 0 && qEco.CalculateEntityCount() > 0)
                {
                    var eco = qEco.GetSingleton<ColonyEcologyIndicatorsState>();
                    var p = EcologyPollutionMath.GetCombinedPollutionPercent0to100(eco);
                    em.CreateSingleton(new ColonyPollutionSummaryState
                    {
                        CombinedPollutionPercent0to100 = p,
                        Band = EcologyPollutionMath.GetPollutionLevelBand(p)
                    });
                }
            }

            using (var q = em.CreateEntityQuery(ComponentType.ReadOnly<AiDirectorDimensionsState>()))
            {
                if (q.CalculateEntityCount() == 0)
                {
                    em.CreateSingleton(new AiDirectorDimensionsState
                    {
                        Wealth0to100 = 50f,
                        Security0to100 = 50f,
                        Stability0to100 = 50f,
                        Progress0to100 = 50f,
                        Tension0to100 = 50f
                    });
                }
            }

            using (var q = em.CreateEntityQuery(ComponentType.ReadOnly<AiDirectorPolicyState>()))
            {
                if (q.CalculateEntityCount() == 0)
                {
                    em.CreateSingleton(new AiDirectorPolicyState
                    {
                        ActivePolicy = AiDirectorPolicyKind.None,
                        LastChangeTick = 0
                    });
                }
            }

        }
    }
}
