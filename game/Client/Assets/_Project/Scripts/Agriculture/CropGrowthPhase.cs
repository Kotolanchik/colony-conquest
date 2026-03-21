namespace ColonyConquest.Agriculture
{
    /// <summary>
    /// Этапы цикла §1.2 <c>spec/agriculture_mining_spec.md</c> (подготовка → посев → рост → уход сросён с ростом → урожай).
    /// </summary>
    public enum CropGrowthPhase : byte
    {
        Preparation = 0,
        Sowing = 1,
        Growth = 2,
        /// <summary>Финальный этап перед сбросом в подготовку; длительность — §1.2 «Урожай — 1 день».</summary>
        Harvest = 3,
    }
}
