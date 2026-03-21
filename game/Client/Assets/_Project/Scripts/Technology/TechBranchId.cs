namespace ColonyConquest.Technology
{
    /// <summary>
    /// Ветки развития технологий — <c>spec/technology_tree_spec.md</c> §1.3.
    /// </summary>
    public enum TechBranchId : byte
    {
        None = 0,
        Military = 1,
        Economic = 2,
        Scientific = 3,
        Social = 4,
        Infrastructure = 5,
    }
}
