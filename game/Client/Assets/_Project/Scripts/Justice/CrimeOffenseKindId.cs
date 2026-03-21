namespace ColonyConquest.Justice
{
    /// <summary>Виды преступлений — <c>spec/crime_justice_spec.md</c> §2.1–2.3 (перечень для данных и симуляции).</summary>
    public enum CrimeOffenseKindId : ushort
    {
        None = 0,

        // §2.1 мелкие
        PettyTheft = 1,
        FoodTheft = 2,
        PropertyDamage = 3,
        Drunkenness = 4,

        // §2.2 серьёзные
        Robbery = 10,
        AssaultGrievous = 11,
        Arson = 12,

        // §2.3 особо тяжкие
        Murder = 20,
        Treason = 21,
        Espionage = 22,
        Sabotage = 23
    }
}
