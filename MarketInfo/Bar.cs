namespace MarketInfo;


/// <summary>
/// Рыночный бар, содержит информацию о времени и о ценах открытия, закрытия, максимума и минимума
/// </summary>
public struct Bar(DateTime date, float open, float close, float high, float low)
{
    public const int BarLength = 8 + 4 + 4 + 4 + 4;//дата + открытие, закрытие, максимум и минимум

    /// <summary>
    /// Дата
    /// </summary>
    public DateTime Date { get; set; } = date;

    /// <summary>
    /// Цена открытия
    /// </summary>
    public float Open { get; set; } = open;

    /// <summary>
    /// Цена закрытия
    /// </summary>
    public float Close { get; set; } = close;

    /// <summary>
    /// Цена максимум
    /// </summary>
    public float High { get; set; } = high;

    /// <summary>
    /// Цена минимума
    /// </summary>
    public float Low { get; set; } = low;


    /// <summary>
    /// Записывает себя в бинарное представление
    /// </summary>
    /// <param name="writer"></param>
    public void Save(BinaryWriter writer)
    {
        writer.Write(Date.Ticks);
        writer.Write(Open);
        writer.Write(Close);
        writer.Write(High);
        writer.Write(Low);
    }
    /// <summary>
    /// Создает новый бар из бинарного представления
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    public static Bar Create(BinaryReader reader)
    {
        return new Bar(new DateTime(reader.ReadInt64()), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
            reader.ReadSingle());
    }
    /// <summary>
    /// Создает массив баров из массива байтов
    /// </summary>
    /// <param name="data">Массив данных одного бара</param>
    /// <returns></returns>
    public static Bar[] Create(byte[] data)
    {
        using MemoryStream ms = new MemoryStream(data);
        var reader = new BinaryReader(ms);
        var arr = new Bar[data.Length / BarLength];
        for (var i = 0; i < arr.Length; i++)
        {
            arr[i] = Create(reader);
        }
        return arr;
    }

    public override string ToString()
    {
        return $"{Date:HH:mm dd.MM.yyyy}";
    }
}
