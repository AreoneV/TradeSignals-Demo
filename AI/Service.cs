using System.Text;
using MarketInfo;
using Protocol;
using Protocol.AI;
using Services;
// ReSharper disable StringLiteralTypo

namespace AI;

internal class Service(string myIp, int myPort)
{
    //Путь к файлу логов
    private const string LogFileName = "logs_ai.txt";
    //логгер и его поток
    private FileStream logStream;
    private StreamWriter writer;
    //максимальная длинна лога для красивой отрисовки
    private int maxLineLength = 1;

    //объект сервера для слушки подключений
    private readonly Server server = new(myIp, myPort, 10);
    //символы для предсказания сигнала
    private readonly Dictionary<string, Symbol> symbols = [];
    //количество входящих баров для предсказания
    public const int InputsBars = 50;

    /// <summary>
    /// Запуск сервиса
    /// </summary>
    /// <returns>Возвращает код работы программы</returns>
    public ExitCode Run()
    {
        try
        {
            logStream = new FileStream(LogFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            logStream.Position = logStream.Length;
            writer = new StreamWriter(logStream, Encoding.UTF8);
        }
        catch
        {
            return ExitCode.ErrorStarting;
        }

        LogInfo("Starting...");

        try
        {
            //создание сетей и загрузка весов
            symbols.Add("EURUSD", new Symbol("EURUSD"));
            symbols.Add("GBPUSD", new Symbol("GBPUSD"));
            LogInfo("Loading neural networks and their weights completed successfully!");
        }
        catch(Exception ex)
        {
            LogError($"Error loading neural networks: {ex}.\n");
            LogSplit();
            return ExitCode.ErrorStarting;
        }

        server.UserConnected += ServerOnUserConnected;
        server.ServerStarted += ServerOnServerStarted;

        try
        {
            //запуск сервера
            server.Start(false);
        }
        catch(Exception e)
        {
            LogError($"Error running server: {e}.\n");
            LogSplit();
            return ExitCode.ErrorStarting;
        }

        LogWarning("Server was stopped!");
        LogSplit();
        writer.Close();


        return ExitCode.Ok;
    }


    /// <summary>
    /// Останавливает и закрывает сервис
    /// </summary>
    /// <param name="reason">Причина остановки</param>
    public void Close(string reason)
    {
        LogWarning($"Server is stopping... Reason: {reason}");
        server.Stop();
    }


    //логирование
    private void LogInfo(string msg)
    {
        var m = $"{DateTime.Now:G} | Info\t| {msg}";
        writer.WriteLine(m);
        writer.Flush();
        if(m.Length > maxLineLength)
        {
            maxLineLength = m.Length;
        }
    }

    private void LogWarning(string msg)
    {
        var m = $"{DateTime.Now:G} | Warn\t| {msg}";
        writer.WriteLine(m);
        writer.Flush();
        if(m.Length > maxLineLength)
        {
            maxLineLength = m.Length;
        }
    }

    private void LogError(string msg)
    {
        var m = $"{DateTime.Now:G} | Error\t| {msg}";
        writer.WriteLine(m);
        writer.Flush();
        if(m.Length > maxLineLength)
        {
            maxLineLength = m.Length;
        }
    }
    //разделитель логов
    private void LogSplit()
    {
        writer.WriteLine(new string('_', maxLineLength));
        writer.WriteLine();
        writer.Flush();
    }



    /// <summary>
    /// Когда сервер запустился
    /// </summary>
    /// <param name="srv"></param>
    private void ServerOnServerStarted(Server srv)
    {
        LogInfo("Server is running...");
    }
    /// <summary>
    /// Когда есть новое соединение
    /// </summary>
    /// <param name="srv"></param>
    /// <param name="client"></param>
    private void ServerOnUserConnected(Server srv, Client client)
    {
        client.ReceivedRequest += ClientOnReceivedRequest;
        client.ClientDisconnected += ClientOnClientDisconnected;
    }
    /// <summary>
    /// Когда соединение разорвано
    /// </summary>
    /// <param name="client"></param>
    private void ClientOnClientDisconnected(Client client)
    {
        client.ReceivedRequest -= ClientOnReceivedRequest;
        client.ClientDisconnected -= ClientOnClientDisconnected;
    }

    /// <summary>
    /// Когда получен новый запрос
    /// </summary>
    /// <param name="client"></param>
    /// <param name="message"></param>
    private void ClientOnReceivedRequest(Client client, byte[] message)
    {
        //данные для считывания
        using var ms = new MemoryStream(message);
        var reader = new BinaryReader(ms);
        //данные для ответа
        using var answer = new MemoryStream();
        var w = new BinaryWriter(answer);
        try
        {
            //получаем команду
            var common = (CommonCommand)reader.ReadInt32();
            switch(common)
            {
                //если команда выключить
                case CommonCommand.Shutdown:
                    //закрываем
                    Close("Received shutdown command!");
                    //отправляем пустой ответ
                    client.SendAnswer([]);
                    break;
                //специальная команда
                case CommonCommand.SpecialCommand:
                    //берем ее и отправляем на обработку
                    var specCommand = (AICommand)reader.ReadInt32();
                    ProcessingCommand(specCommand, reader, w);
                    //отправляем ответ
                    client.SendAnswer(answer.ToArray());
                    break;
                case CommonCommand.Logs:
                    
                    //проверяем есть ли логи
                    if(logStream.Length == 0)
                    {
                        w.Write("There isn't logs");
                        client.SendAnswer(answer.ToArray());
                        return;
                    }
                    //считываем и отправляем
                    logStream.Position = 0;
                    var buf = new byte[logStream.Length];
                    logStream.Read(buf, 0, buf.Length);
                    w.Write(buf);
                    client.SendAnswer(answer.ToArray());
                    return;
                case CommonCommand.Ping:
                case CommonCommand.Ok:
                case CommonCommand.Error:
                default:
                    //в остальных случаях просто отправляем то что нам прислали
                    client.SendAnswer(message);
                    return;
            }
        }
        catch(Exception ex)
        {
            LogError($"Error processing command: {ex.Message}");
            w.Write(ex.ToString());
            client.SendAnswer(answer.ToArray());
        }
    }
    /// <summary>
    /// Обработка специальных запросов
    /// </summary>
    /// <param name="command">Команда</param>
    /// <param name="reader">Считывание данных которые пришли вместе с командой</param>
    /// <param name="w">Запись данных для отправки ответа</param>
    private void ProcessingCommand(AICommand command, BinaryReader reader, BinaryWriter w)
    {
        switch(command)
        {
            //запрос на получение символов
            case AICommand.Predict:
                //считываем символ и временную рамку
                string symbol = reader.ReadString();
                var tf = (TimeFrame)reader.ReadInt32();
                //получаем длину входящих данных
                int len = reader.ReadInt32();
                //создаем массив данных
                float[] data = new float[len];
                //считываем данные в массив
                for (int i = 0; i < len; i++)
                {
                    data[i] = reader.ReadSingle();
                }

                try
                {
                    //предсказываем сигнал по рыночным данным 
                    var p = symbols[symbol].Predict(tf, data);
                    //пишем предсказания
                    w.Write(p.buy);
                    w.Write(p.sell);
                }
                catch
                {
                    //возвращаем нули если чего то нет
                    w.Write(0f);
                    w.Write(0f);
                }
                break;
            default:
                w.Write(0f);
                w.Write(0f);
                break;
        }
    }
}