namespace ColonyConquest.Agriculture
{
    /// <summary>Скорости сбора при удержании Interact — §2.3 (биомасса → склад).</summary>
    public static class WildRenewableGatherTuning
    {
        public const float GatherRadius = 4f;

        /// <summary>Единиц улова на игровой час (ограничено текущей рыбной биомассой).</summary>
        public const float FishCatchPerGameHour = 10f;

        /// <summary>Единиц мяса на игровой час (ограничено биомассой дичи).</summary>
        public const float WildGameMeatPerGameHour = 4f;
    }
}
