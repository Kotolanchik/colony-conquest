using ColonyConquest.Analytics;
using ColonyConquest.Ecology;
using ColonyConquest.Economy;
using ColonyConquest.Military;
using ColonyConquest.Settlers;
using ColonyConquest.Simulation;
using ColonyConquest.Story;
using ColonyConquest.Technology;
using ColonyConquest.UI;
using ColonyConquest.WorldMap;
using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Audio
{
    /// <summary>Full runtime аудиосистемы: adaptive music, ingest шины событий, бюджет голосов и 3D-каналов.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(UiUxRuntimeSystem))]
    public partial struct AudioSimulationRuntimeSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AudioSimulationSingleton>();
            state.RequireForUpdate<AudioSimulationState>();
            state.RequireForUpdate<AudioBusServiceSingleton>();
            state.RequireForUpdate<GameCalendarState>();
            state.RequireForUpdate<SimulationRootState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            ref var sim = ref SystemAPI.GetSingletonRW<AudioSimulationState>().ValueRW;
            var calendar = SystemAPI.GetSingleton<GameCalendarState>();
            var day = calendar.DayIndex;
            var isNewDay = sim.LastProcessedDay != day;

            var emitters = SystemAPI.GetSingletonBuffer<AudioActiveEmitterEntry>(ref state);
            var transitions = SystemAPI.GetSingletonBuffer<AudioMusicTransitionEntry>(ref state);
            var bus = SystemAPI.GetSingletonBuffer<AudioBusPendingEntry>(ref state);

            var themeEra = (byte)1;
            if (SystemAPI.HasSingleton<ColonyTechProgressState>())
                themeEra = AudioSimulationMath.ToThemeEra(SystemAPI.GetSingleton<ColonyTechProgressState>().CurrentEra);

            var cameraLevel = UiCameraLevel.Tactical;
            if (SystemAPI.HasSingleton<UiUxSimulationState>())
            {
                var ui = SystemAPI.GetSingleton<UiUxSimulationState>();
                cameraLevel = ui.CameraLevel;
                sim.SoundSignalsEnabled = ui.SoundSignalsEnabled;
            }

            var readiness01 = 0f;
            uint activeUnits = 0;
            var supplyAdequacy01 = 1f;
            if (SystemAPI.HasSingleton<MilitarySimulationState>())
            {
                var military = SystemAPI.GetSingleton<MilitarySimulationState>();
                readiness01 = military.CombatReadiness01;
                activeUnits = military.ActiveArmyUnits;
                supplyAdequacy01 = military.SupplyAdequacy01;
            }

            var tension01 = 0f;
            if (SystemAPI.HasSingleton<StorySimulationState>())
                tension01 = SystemAPI.GetSingleton<StorySimulationState>().StoryTension01;

            var morale01 = 1f;
            var foodSatisfied01 = 1f;
            if (SystemAPI.HasSingleton<SettlerSimulationState>())
            {
                var settlers = SystemAPI.GetSingleton<SettlerSimulationState>();
                morale01 = settlers.ColonyMorale01;
                foodSatisfied01 = settlers.ResourceFoodSatisfied01;
            }

            var economyEfficiency01 = 1f;
            if (SystemAPI.HasSingleton<EconomySimulationState>())
                economyEfficiency01 = SystemAPI.GetSingleton<EconomySimulationState>().ProductionEfficiency01;

            var ecology01 = 1f;
            if (SystemAPI.HasSingleton<ColonyEcologyIndicatorsState>())
            {
                var eco = SystemAPI.GetSingleton<ColonyEcologyIndicatorsState>();
                ecology01 = (eco.AirQuality01 + eco.WaterQuality01 + eco.Biodiversity01) / 3f;
            }

            var weatherSeverity01 = 0f;
            if (SystemAPI.HasSingleton<MilitaryEnvironmentState>())
                weatherSeverity01 = SystemAPI.GetSingleton<MilitaryEnvironmentState>().WeatherSeverity01;

            var combat01 = AudioSimulationMath.ComputeCombatIntensity01(readiness01, activeUnits, supplyAdequacy01, tension01);
            var crisis01 = AudioSimulationMath.ComputeBaseCrisis01(morale01, foodSatisfied01, economyEfficiency01, ecology01);
            var musicIntensity01 = AudioSimulationMath.ComputeMusicIntensity01(combat01, crisis01, calendar.HourOfDay, cameraLevel);
            var targetLevel = sim.AdaptiveMusicEnabled != 0 ? AudioSimulationMath.ToMusicLevel(musicIntensity01) : sim.MusicLevel;

            if (targetLevel != sim.MusicLevel || themeEra != sim.ThemeEra)
            {
                var crossfade = AudioSimulationMath.ComputeCrossfadeSeconds(sim.MusicLevel, targetLevel);
                transitions.Add(new AudioMusicTransitionEntry
                {
                    DayIndex = day,
                    SimulationTick = SystemAPI.GetSingleton<SimulationRootState>().SimulationTick,
                    ThemeEra = themeEra,
                    FromLevel = sim.MusicLevel,
                    ToLevel = targetLevel,
                    CrossfadeSeconds = crossfade
                });
                sim.MusicTransitionsTotal++;
                sim.MusicCrossfadeSeconds = crossfade;
                sim.MusicLevel = targetLevel;
            }

            sim.ThemeEra = themeEra;
            sim.CombatIntensity01 = combat01;
            sim.BaseCrisis01 = crisis01;
            sim.MusicIntensity01 = musicIntensity01;
            sim.LastWeatherSeverity01 = weatherSeverity01;

            var biome = WorldBiomeId.MixedForest;
            if (SystemAPI.HasSingleton<WorldMapFocusState>())
                biome = SystemAPI.GetSingleton<WorldMapFocusState>().PreviewBiome;

            ConsumeAudioBus(ref sim, weatherSeverity01, biome, ref bus, ref emitters);
            TickEmitters(ref sim, ref emitters, (float)SystemAPI.Time.DeltaTime);

            sim.ActiveVoices = (uint)emitters.Length;
            sim.Active3dSources = Count3d(ref emitters);
            sim.EstimatedMemoryMb = AudioSimulationMath.EstimateMemoryMb(sim.ActiveVoices, sim.Active3dSources);
            sim.EstimatedLatencyMs = AudioSimulationMath.EstimateLatencyMs(sim.ActiveVoices, sim.ConcurrentVoiceBudget);

            if (isNewDay)
            {
                sim.LastProcessedDay = day;
                AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.AudioMusicIntensity01,
                    sim.MusicIntensity01);
                AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.AudioMusicLevel, (byte)sim.MusicLevel);
                AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.AudioActiveVoices, sim.ActiveVoices);
                AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.AudioActive3dSources,
                    sim.Active3dSources);
                AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.AudioDroppedEventsTotal,
                    sim.DroppedBusEventsTotal);
                AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.AudioEstimatedMemoryMb,
                    sim.EstimatedMemoryMb);
                AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.AudioEstimatedLatencyMs,
                    sim.EstimatedLatencyMs);
            }
        }

        static void ConsumeAudioBus(ref AudioSimulationState sim, float weatherSeverity01, WorldBiomeId biome,
            ref DynamicBuffer<AudioBusPendingEntry> bus, ref DynamicBuffer<AudioActiveEmitterEntry> emitters)
        {
            var reverb = AudioSimulationMath.ComputeBiomeReverbSeconds(biome);
            for (var i = 0; i < bus.Length; i++)
            {
                var pending = bus[i];
                if (pending.Category == AudioSfxCategory.Interface && sim.SoundSignalsEnabled == 0)
                    continue;

                var candidate = new AudioActiveEmitterEntry
                {
                    EventId = pending.EventId,
                    Category = pending.Category,
                    Position = pending.WorldPosition,
                    HasWorldPosition = pending.HasWorldPosition,
                    Priority01 = AudioSimulationMath.ComputeCategoryPriority01(pending.Category),
                    Volume01 = sim.MasterVolume01 * sim.SfxVolume01,
                    RemainingSeconds = AudioSimulationMath.ComputeLifetimeSeconds(pending.Category),
                    Occlusion01 = AudioSimulationMath.ComputeOcclusion01(weatherSeverity01, pending.HasWorldPosition != 0),
                    ReverbSeconds = reverb
                };

                if (emitters.Length >= sim.ConcurrentVoiceBudget)
                {
                    var replaceAt = FindLowestPriorityIndex(ref emitters);
                    if (replaceAt < 0 || emitters[replaceAt].Priority01 >= candidate.Priority01)
                    {
                        sim.DroppedBusEventsTotal++;
                        continue;
                    }

                    emitters[replaceAt] = candidate;
                }
                else
                {
                    emitters.Add(candidate);
                }

                sim.ConsumedBusEventsTotal++;
            }

            bus.Clear();
        }

        static int FindLowestPriorityIndex(ref DynamicBuffer<AudioActiveEmitterEntry> emitters)
        {
            if (emitters.Length == 0)
                return -1;

            var bestIdx = 0;
            var lowest = emitters[0].Priority01;
            for (var i = 1; i < emitters.Length; i++)
            {
                if (emitters[i].Priority01 >= lowest)
                    continue;
                lowest = emitters[i].Priority01;
                bestIdx = i;
            }

            return bestIdx;
        }

        static void TickEmitters(ref AudioSimulationState sim, ref DynamicBuffer<AudioActiveEmitterEntry> emitters, float deltaTime)
        {
            for (var i = emitters.Length - 1; i >= 0; i--)
            {
                var entry = emitters[i];
                entry.RemainingSeconds -= math.max(0f, deltaTime);
                if (entry.RemainingSeconds <= 0f)
                {
                    emitters.RemoveAt(i);
                    continue;
                }

                var volume = sim.MasterVolume01 * sim.SfxVolume01;
                if (entry.HasWorldPosition != 0 && sim.SpatialAudioEnabled != 0)
                {
                    var distance = math.length(entry.Position);
                    volume *= AudioSimulationMath.ComputeAttenuation01(distance);
                    volume *= 1f - entry.Occlusion01;
                }

                entry.Volume01 = math.saturate(volume);
                emitters[i] = entry;
            }

            var active3d = Count3d(ref emitters);
            while (active3d > sim.Concurrent3dBudget)
            {
                var removeIndex = FindLowestPriority3dIndex(ref emitters);
                if (removeIndex < 0)
                    break;
                var downgraded = emitters[removeIndex];
                downgraded.HasWorldPosition = 0;
                downgraded.Position = float3.zero;
                downgraded.Occlusion01 = 0f;
                emitters[removeIndex] = downgraded;
                active3d--;
            }
        }

        static int FindLowestPriority3dIndex(ref DynamicBuffer<AudioActiveEmitterEntry> emitters)
        {
            var found = -1;
            var lowest = 2f;
            for (var i = 0; i < emitters.Length; i++)
            {
                if (emitters[i].HasWorldPosition == 0)
                    continue;
                if (emitters[i].Priority01 >= lowest)
                    continue;
                lowest = emitters[i].Priority01;
                found = i;
            }

            return found;
        }

        static uint Count3d(ref DynamicBuffer<AudioActiveEmitterEntry> emitters)
        {
            uint count = 0;
            for (var i = 0; i < emitters.Length; i++)
            {
                if (emitters[i].HasWorldPosition != 0)
                    count++;
            }

            return count;
        }
    }
}
