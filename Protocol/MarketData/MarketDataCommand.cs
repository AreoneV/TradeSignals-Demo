namespace Protocol.MarketData;
public enum MarketDataCommand
{
    /// <summary>
    /// Запросить бары
    /// </summary>
    GetBars,
    /// <summary>
    /// Запросить первую и последнюю даты
    /// </summary>
    GetExtremeDate,
    /// <summary>
    /// Запросить последний бар
    /// </summary>
    GetLasBar,
    /// <summary>
    /// Запросить имена всех символов
    /// </summary>
    GetSymbolNames,
}
