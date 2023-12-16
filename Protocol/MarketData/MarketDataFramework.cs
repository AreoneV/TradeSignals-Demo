using MarketInfo;
using System.Diagnostics;
using System.Text;

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
    public (DateTime first, DateTime last) GetExtremeDate(string symbol, TimeFrame timeFrame)
    {
        //проверка соединения
        if(!client.IsConnected)
        {
            client.Connect();
        }

        //создаем бинарное представление команды для сервиса
        using MemoryStream memoryStream = new();
        BinaryWriter writer = new(memoryStream);
        writer.Write((int)CommonCommand.SpecialCommand);
        writer.Write((int)MarketDataCommand.GetExtremeDate);
        writer.Write(symbol);
        writer.Write((int)timeFrame);
        //отправляем и получаем ответ
        var answer = client.Request(memoryStream.ToArray());
        writer.Close();
        memoryStream.Close();
        //создаем чтение и читаем из бинарного представление данные
        using MemoryStream answerStream = new(answer);
        var reader = new BinaryReader(answerStream);

        var first = new DateTime(reader.ReadInt64());
        var last = new DateTime(reader.ReadInt64());
        //все закрываем
        reader.Close();
        answerStream.Close();
        //возвращаем первую и последнюю даты
        return (first, last);
    }
    /// <summary>
    /// Получить самый последний бар
    /// </summary>
    /// <param name="timeFrame">Временная рамка для получения последнего бара</param>
    /// <returns>Последний бар</returns>
    public Bar GetLastBar(string symbol, TimeFrame timeFrame)
    {
        //проверка соединения
        if(!client.IsConnected)
        {
            client.Connect();
        }
        //создаем бинарное представление команды для сервиса
        using MemoryStream memoryStream = new();
        BinaryWriter writer = new(memoryStream);
        writer.Write((int)CommonCommand.SpecialCommand);
        writer.Write((int)MarketDataCommand.GetLasBar);
        writer.Write(symbol);
        writer.Write((int)timeFrame);
        //отправляем и получаем ответ
        var answer = client.Request(memoryStream.ToArray());
        writer.Close();
        memoryStream.Close();
        //создаем чтение и читаем из бинарного представление данные
        using MemoryStream answerStream = new(answer);
        var reader = new BinaryReader(answerStream);

        var b = Bar.Create(reader);
        //все закрываем
        reader.Close();
        answerStream.Close();
        //возвращаем последний бар
        return b;

    }
    /// <summary>
    /// Получить историю баров
    /// </summary>
    /// <param name="timeFrame">Временная рамка</param>
    /// <param name="count">Количество баров</param>
    /// <returns>Массив последних баров</returns>
    public Bar[] GetBars(string symbol, TimeFrame timeFrame, int count)
    {
        //проверка соединения
        if(!client.IsConnected)
        {
            client.Connect();
        }
        //создаем бинарное представление команды для сервиса
        using MemoryStream memoryStream = new();
        BinaryWriter writer = new(memoryStream);
        writer.Write((int)CommonCommand.SpecialCommand);
        writer.Write((int)MarketDataCommand.GetBars);
        writer.Write(symbol);
        writer.Write((int)timeFrame);
        writer.Write(count);
        //отправляем и получаем ответ
        var answer = client.Request(memoryStream.ToArray());
        writer.Close();
        memoryStream.Close();
        //создаем чтение и читаем из бинарного представление данные
        using MemoryStream answerStream = new(answer);
        var reader = new BinaryReader(answerStream);
        //получаем длинну
        var len = reader.ReadInt32();
        var bars = new Bar[len];
        for (int i = 0; i < len; i++)
        {
            //считываем каждый бар и записываем
            bars[i] = Bar.Create(reader);
        }
        //все закрываем
        reader.Close();
        answerStream.Close();
        //возвращаем бары
        return bars;
    }
    /// <summary>
    /// Получить все имена символов
    /// </summary>
    /// <returns>Массив имен символов</returns>
    public string[] GetSymbolNames()
    {
        //проверка соединения
        if(!client.IsConnected)
        {
            client.Connect();
        }
        //создаем бинарное представление команды для сервиса
        using MemoryStream memoryStream = new();
        BinaryWriter writer = new(memoryStream);
        writer.Write((int)CommonCommand.SpecialCommand);
        writer.Write((int)MarketDataCommand.GetSymbolNames);
        //отправляем и получаем ответ
        var answer = client.Request(memoryStream.ToArray());
        writer.Close();
        memoryStream.Close();
        //создаем чтение и читаем из бинарного представление данные
        using MemoryStream answerStream = new(answer);
        var reader = new BinaryReader(answerStream);
        //порлучаем длинну
        var len = reader.ReadInt32();
        var symbols = new string[len];
        for(int i = 0; i < len; i++)
        {
            //читаем каждое имя
            symbols[i] = reader.ReadString();
        }
        //все закрываем
        reader.Close();
        answerStream.Close();
        //возвращаем символы
        return symbols;
    }
    /// <summary>
    /// Получить все логи с сервиса
    /// </summary>
    /// <returns>Массив логов</returns>
    public string[] GetLogs()
    {
        //проверка соединения
        if(!client.IsConnected)
        {
            client.Connect();
        }
        //создаем бинарное представление команды для сервиса
        using MemoryStream memoryStream = new();
        BinaryWriter writer = new(memoryStream);
        writer.Write((int)CommonCommand.Logs);
        //отправляем и получаем ответ
        var answer = client.Request(memoryStream.ToArray());
        writer.Close();
        memoryStream.Close();

        //получаем все логи и делим их
        var logs = Encoding.UTF8.GetString(answer).Split('\n');
        //возвращаем логи
        return logs;
    }
    /// <summary>
    /// Проверяет скорость соединения(запрос - ответ) в миллисекундах
    /// </summary>
    /// <param name="packetLength">Длинна пакета в байтах</param>
    /// <returns>Время в миллисекундах</returns>
    public double Ping(int packetLength)
    {
        //проверка соединения
        if(!client.IsConnected)
        {
            client.Connect();
        }
        //создаем бинарное представление команды для сервиса изапускаем таймер
        var sw = Stopwatch.StartNew();
        using MemoryStream memoryStream = new();
        BinaryWriter writer = new(memoryStream);
        writer.Write((int)CommonCommand.Ping);
        writer.Write(new byte[packetLength]);
        //отправляем и получаем ответ
        var answer = client.Request(memoryStream.ToArray());

        //все закрываем и останавливаем таймер
        sw.Stop();
        writer.Close();
        memoryStream.Close();
        //возвращаем время в миллисекундах
        return sw.Elapsed.TotalMilliseconds;
    }
    /// <summary>
    /// Посылает команду на выключение сервиса
    /// </summary>
    public void Shutdown()
    {
        //проверка соединения
        if(!client.IsConnected)
        {
            client.Connect();
        }
        //создаем бинарное представление команды для сервиса
        using MemoryStream memoryStream = new();
        BinaryWriter writer = new(memoryStream);
        writer.Write((int)CommonCommand.Shutdown);
        //отправляем и получаем пустой ответ
        client.Request(memoryStream.ToArray());
        //все закрываем
        writer.Close();
        memoryStream.Close();
        client.Close();
    }
}
