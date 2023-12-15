using MarketInfo;
using Protocol;
using Protocol.MarketData;
using Services;

namespace AI;

public class Service(string myIp, int myPort)
{
    //Путь к файлу логов
    private const string LogFileName = "logs_ai.txt";
    //логгер
    private readonly StreamWriter writer = new(LogFileName, true);
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
                    
                    //отправляем ответ
                    client.SendAnswer(answer.ToArray());
                    break;
                case CommonCommand.Logs:
                    //проверяем есть ли файл
                    if(!File.Exists(LogFileName))
                    {
                        w.Write("There isn't logs");
                        client.SendAnswer(answer.ToArray());
                        return;
                    }
                    //считываем и отправляем
                    w.Write(File.ReadAllText(LogFileName));
                    client.SendAnswer(answer.ToArray());
                    return;
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
    
}