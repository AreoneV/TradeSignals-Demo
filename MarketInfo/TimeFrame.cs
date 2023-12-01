// ReSharper disable InconsistentNaming
namespace MarketInfo;


/// <summary>
/// Временные рамки для построения графика
/// </summary>
public enum TimeFrame
{
    /// <summary>
    /// 1 минута
    /// </summary>
    M1 = 1,  
    /// <summary>
    /// 5 минут
    /// </summary>
    M5 = 5,
    /// <summary>
    /// 15 минут
    /// </summary>
    M15 = 15,
    /// <summary>
    /// 30 минут
    /// </summary>
    M30 = 30,
    /// <summary>
    /// 1 час
    /// </summary>
    H1 = 60,
    /// <summary>
    /// 4 часа
    /// </summary>
    H4 = 240,
    /// <summary>
    /// 1 день
    /// </summary>
    D1 = 1440,
    /// <summary>
    /// 1 неделя
    /// </summary>
    W1 = 10080,
    /// <summary>
    /// 1 месяц
    /// </summary>
    MN1 = 43200
}