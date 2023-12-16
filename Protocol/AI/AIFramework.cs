using MarketInfo;
using Protocol.MarketData;
using System.Diagnostics;
using System.Text;

namespace Protocol.AI;

/// <summary>
/// Библиотека для упрощенного использования сервиса AI
/// </summary>
/// <param name="ip">IP адрес сервиса</param>
/// <param name="port">Порт сервиса</param>
public class AIFramework(string ip, int port)
{
    /// <summary>
    /// Подключение к сервису
    /// </summary>
    private readonly Client client = new(ip, port);




    public (float buyPrediction, float sellPrediction) Predict(string symbol, TimeFrame timeFrame, float[] bars)
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
        writer.Write((int)AICommand.Predict);
        writer.Write(symbol);
        writer.Write((int)timeFrame);

        //записываем данные рынка для прогноза
        writer.Write(bars.Length);
        foreach (float bar in bars)
        {
            writer.Write(bar);
        }
        //отправляем и получаем ответ
        var answer = client.Request(memoryStream.ToArray());
        writer.Close();
        memoryStream.Close();
        //создаем чтение и читаем из бинарного представление данные
        using MemoryStream answerStream = new(answer);
        var reader = new BinaryReader(answerStream);

        var buy = reader.ReadSingle();
        var sell = reader.ReadSingle();
        //все закрываем
        reader.Close();
        answerStream.Close();
        //возвращаем предсказания по buy и sell направлениям
        return (buy, sell);
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
        //создаем бинарное представление команды для сервиса и запускаем таймер
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