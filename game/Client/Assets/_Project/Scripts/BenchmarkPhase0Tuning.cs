namespace ColonyConquest.Core
{
    /// <summary>
    /// Единственная точка включения замера производительности фазы 0 (мастер-спека §6.1: ориентир 1000 сущностей / 60 FPS).
    /// По умолчанию выключено, чтобы не нагружать машину без явного намерения.
    /// </summary>
    public static class BenchmarkPhase0Tuning
    {
        /// <summary>Поставьте <c>true</c> перед Play для спавна нагрузочных сущностей и отчёта в консоль.</summary>
        public const bool Enabled = false;
    }
}
