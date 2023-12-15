namespace AI;

public class Service
{
    //Путь к файлу логов
    private const string LogFileName = "logs_ai.txt";
    //логгер
    private readonly StreamWriter writer = new(LogFileName, true);
    //максимальная длинна лога для красивой отрисовки
    private int maxLineLength = 1;




    public const int InputsBars = 50;




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