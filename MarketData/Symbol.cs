using System.Collections.ObjectModel;
using MarketInfo;

namespace MarketData;

/// <summary>
/// Хранит историю рыночного символа по разным временным рамкам
/// </summary>
public class Symbol
{
    private readonly Dictionary<TimeFrame, History> histories = new Dictionary<TimeFrame, History>(9);



    public Symbol(string name, int digits)
    {
        Name = name;
        Digits = digits;

        foreach (var timeFrame in Enum.GetValues<TimeFrame>())
        {
            histories.Add(timeFrame, new History(timeFrame));
        }

        Histories = histories.AsReadOnly();
    }

    /// <summary>
    /// Имя символа
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// Количество цифр после запятой
    /// </summary>
    public int Digits { get; }

    /// <summary>
    /// Истории разных временных рамок
    /// </summary>
    public ReadOnlyDictionary<TimeFrame, History> Histories { get; }
}