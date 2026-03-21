namespace ColonyConquest.Ecology
{
    /// <summary>Диапазон общего загрязнения 0–100; <c>spec/ecology_spec.md</c> §3.1–3.3.</summary>
    public enum PollutionLevelBand : byte
    {
        /// <summary>0–20</summary>
        Clean = 0,
        /// <summary>21–40</summary>
        Low = 1,
        /// <summary>41–60</summary>
        Medium = 2,
        /// <summary>61–80</summary>
        High = 3,
        /// <summary>81–100</summary>
        Critical = 4
    }
}
