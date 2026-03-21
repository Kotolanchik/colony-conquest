namespace ColonyConquest.Technology
{
    /// <summary>
    /// Эпохи дерева технологий — <c>spec/technology_tree_spec.md</c> §1.2.
    /// </summary>
    public enum TechEraId : byte
    {
        None = 0,
        /// <summary>XVI–XVII век</summary>
        Era1_Foundation = 1,
        /// <summary>XIX век</summary>
        Era2_Industrialization = 2,
        /// <summary>1900–1918</summary>
        Era3_WorldWar1 = 3,
        /// <summary>1918–1945</summary>
        Era4_WorldWar2 = 4,
        /// <summary>1945+</summary>
        Era5_ModernFuture = 5,
    }
}
