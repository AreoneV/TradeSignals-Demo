using Protocol;
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
}