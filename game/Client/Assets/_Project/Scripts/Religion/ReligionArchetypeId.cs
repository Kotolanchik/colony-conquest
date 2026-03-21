namespace ColonyConquest.Religion
{
    /// <summary>
    /// Типы религий и близкие категории — <c>spec/religion_cults_spec.md</c> §1.2 (организованные, культы, атеизм/секуляризм).
    /// </summary>
    public enum ReligionArchetypeId : byte
    {
        None = 0,
        Monotheism = 1,
        Polytheism = 2,
        /// <summary>Духи природы</summary>
        AnimismNature = 3,
        /// <summary>Предки</summary>
        AnimismAncestors = 4,
        PrehistoricCult = 5,
        SecretSociety = 6,
        DestructiveCult = 7,
        PersonalityCult = 8,
        Atheism = 9,
        Secularism = 10,
        ScientificMaterialism = 11,
    }
}
