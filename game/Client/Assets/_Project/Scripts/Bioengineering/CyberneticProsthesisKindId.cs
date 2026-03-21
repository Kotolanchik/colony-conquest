namespace ColonyConquest.Bioengineering
{
    /// <summary>
    /// Типы кибер-/протезов из таблицы §2.2 <c>spec/bioengineering_spec.md</c> (идентификаторы данных, без симуляции установки).
    /// </summary>
    public enum CyberneticProsthesisKindId : byte
    {
        None = 0,
        WoodenLeg = 1,
        Hook = 2,
        MechanicalArm = 3,
        MechanicalLeg = 4,
        BionicArm = 5,
        BionicLeg = 6,
        CombatProsthesis = 7,
        CyberEyes = 8,
        ArtificialHeart = 9,
        ArtificialLungs = 10,
    }
}
