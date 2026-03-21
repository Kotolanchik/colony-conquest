namespace ColonyConquest.Politics
{
    /// <summary>
    /// Узлы политического спектра §1.2 и основные течения §2–3 <c>spec/political_system_spec.md</c>.
    /// </summary>
    public enum PoliticalDoctrineId : byte
    {
        None = 0,
        Monarchy = 1,
        Conservatism = 2,
        Liberalism = 3,
        Communism = 4,
        Fascism = 5,
        Anarchism = 6,
        /// <summary>Центр спектра (диаграмма §1.2)</summary>
        Centrism = 7,
        /// <summary>§3.1</summary>
        ConservativeLiberal = 8,
        /// <summary>§3.2</summary>
        SocialDemocracy = 9,
    }
}
