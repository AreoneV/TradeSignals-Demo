using MarketInfo;

namespace Protocol.MarketData;
/// <summary>
/// Библиотека для упрощенного использования сервиса MarketData
/// </summary>
/// <param name="ip">IP адрес сервиса</param>
/// <param name="port">Порт сервиса</param>
public class MarketDataFramework(string ip, int port)
{
    /// <summary>
    /// Подключение к сервису
    /// </summary>
    private readonly Client client = new(ip, port);



    /// <summary>
    /// Получить крайние даты(самую первую и самую последнюю)
    /// </summary>
    /// <param name="timeFrame">Временная рамка для получения</param>
    /// <returns>Первую и последнюю извесную дату</returns>
    public (DateTime first, DateTime last) GetExtremeDate(TimeFrame timeFrame)
    {
        if(!client.IsConnected)
        {
            client.Connect();
        }

        using MemoryStream memoryStream = new();
        BinaryWriter writer = new(memoryStream);
        writer.Write((int)CommonCommand.SpecialCommand);
        writer.Write((int)MarketDataCommand.GetExtremeDate);
        writer.Write((int)timeFrame);

        var answer = client.Request(memoryStream.ToArray());
        writer.Close();
        memoryStream.Close();

        using MemoryStream answerStream = new(answer);
        var reader = new BinaryReader(answerStream);

        var first = new DateTime(reader.ReadInt64());
        var last = new DateTime(reader.ReadInt64());

        reader.Close();
        answerStream.Close();

        return (first, last);
    }
    /// <summary>
    /// Получить самый последний бар
    /// </summary>
    /// <param name="timeFrame">Временная рамка для получения последнего бара</param>
    /// <returns>Последний бар</returns>
    public Bar GetLastBar(TimeFrame timeFrame)
    {
        if(!client.IsConnected)
        {
            client.Connect();
        }

        using MemoryStream memoryStream = new();
        BinaryWriter writer = new(memoryStream);
        writer.Write((int)CommonCommand.SpecialCommand);
        writer.Write((int)MarketDataCommand.GetLasBar);
        writer.Write((int)timeFrame);

        var answer = client.Request(memoryStream.ToArray());
        writer.Close();
        memoryStream.Close();

        using MemoryStream answerStream = new(answer);
        var reader = new BinaryReader(answerStream);

        var b = Bar.Create(reader);

        reader.Close();
        answerStream.Close();

        return b;

    }
    /// <summary>
    /// Получить историю баров
    /// </summary>
    /// <param name="timeFrame">Временная рамка</param>
    /// <param name="count">Количество баров</param>
    /// <returns>Массив последних баров</returns>
    public Bar[] GetBars(TimeFrame timeFrame, int count)
    {
        if(!client.IsConnected)
        {
            client.Connect();
        }

        using MemoryStream memoryStream = new();
        BinaryWriter writer = new(memoryStream);
        writer.Write((int)CommonCommand.SpecialCommand);
        writer.Write((int)MarketDataCommand.GetLasBar);
        writer.Write((int)timeFrame);
        writer.Write(count);

        var answer = client.Request(memoryStream.ToArray());
        writer.Close();
        memoryStream.Close();

        using MemoryStream answerStream = new(answer);
        var reader = new BinaryReader(answerStream);

        var len = reader.ReadInt32();
        var bars = new Bar[len];
        for (int i = 0; i < len; i++)
        {
            bars[i] = Bar.Create(reader);
        }

        reader.Close();
        answerStream.Close();

        return bars;
    }
}
