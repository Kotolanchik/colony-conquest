namespace ColonyConquest.Entertainment
{
    /// <summary>
    /// Репрезентативные подтипы из блоков §1.2 <c>spec/entertainment_spec.md</c> (не исчерпывающий перечень видов спорта и искусства).
    /// </summary>
    public enum EntertainmentActivityKindId : byte
    {
        None = 0,
        TeamSports = 1,
        IndividualSports = 2,
        MartialArts = 3,
        Racing = 4,
        Music = 5,
        Painting = 6,
        Literature = 7,
        Theater = 8,
        Cinema = 9,
        BoardGames = 10,
        Cards = 11,
        Gambling = 12,
        VideoGames = 13,
        Tavern = 14,
        Holidays = 15,
        Dances = 16,
        Picnics = 17,
    }
}
