using Services;

namespace Controller;

internal class Program
{
    private static readonly ServiceManagement Management = ServiceManagement.GetInstance();


    private static void Main()
    {
        Console.Title = "Service controller";
        Console.Write("Loading...");
        //загружаем все настройки
        Management.Load();
        Console.WriteLine();
        //выводим шапку о сервисах
        WriteTable.WriteHeader();
        //выводим инфу о сервисах
        foreach ((ServiceNames _, ServiceObject value) in Management.Services)
        {
            WriteTable.WriteRow(value.Name.ToString(), value.FullPath, value.Ip, value.AutoStart.ToString());
            WriteTable.WriteFill();
        }
        //выводим инфу о запуске
        Console.WriteLine();
        Management.WriteInfo();
        //начинаем получать команды от пользователя
        Console.WriteLine("If you need help enter '?' or 'help'");
        Console.Write("Enter command:");
        WhileCommand();
        //закрываем логгер
        ServiceManagement.LogClose();
    }


    
    private static void WhileCommand()
    {
        while (true)
        {
            //получаем команду и делим ее на поля
            var cmdLine = Console.ReadLine()?.Split(' ');
            if (cmdLine == null)
            {
                continue;
            }

            (int Left, int Top) pos = Console.GetCursorPosition();
            //стираем команду из консоли
            Console.SetCursorPosition(0, pos.Top - 1);
            Console.WriteLine(new string(' ', 100));
            Console.SetCursorPosition(0, pos.Top - 1);
            //проверяем команду
            var cmd = cmdLine[0];
            switch(cmd)
            {
                case "exit":
                    //если выход то останавливаем и выходим
                    ServiceManagement.LogInfo("Enter 'exit' command");
                    Management.Stop();
                    ServiceManagement.LogSplit();
                    return;
                case "stop":
                    //если остановка то просто останавливаем
                    ServiceManagement.LogInfo("Enter 'stop' command");
                    Management.Stop();
                    ServiceManagement.LogSplit();
                    break;
                case "start":
                    //если запуск то запускаем
                    ServiceManagement.LogInfo("Enter 'start' command");
                    Management.Start();
                    ServiceManagement.LogSplit();
                    break;
                case "help":
                case "?":
                    //если команда помощь выведем инфу
                    var empty = new string(' ', 4);
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write($"{empty}start");
                    Console.ResetColor();
                    Console.WriteLine(" - Starting all services which auto start property is true.");
                    Console.WriteLine();

                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write($"{empty}stop");
                    Console.ResetColor();
                    Console.WriteLine(" - Stopping all services.");
                    Console.WriteLine();

                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write($"{empty}service ");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write("[name]  [command]");
                    Console.ResetColor();
                    Console.WriteLine(" - Service management. Stopping, Starting and Information");
                    Console.WriteLine($"{empty}stop - stopping the service; start - starting the service");
                    Console.WriteLine($"{empty}ping - check connection ping");
                    Console.WriteLine();

                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write($"{empty}clear");
                    Console.ResetColor();
                    Console.WriteLine(" - Clearing console.");
                    Console.WriteLine();

                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write($"{empty}exit");
                    Console.ResetColor();
                    Console.WriteLine(" - Stopping and exiting of all.");
                    Console.WriteLine();

                    break;
                case "clear":
                    //если очистить то очищаем
                    Management.Clear();
                    break;
                case "service":
                    //если команда обращения к сервису

                    //если нет запуска
                    if (!Management.IsStarted)
                    {
                        Console.WriteLine("Controller is stopped.");
                        break;
                    }
                    //если формат не соответствует
                    if (cmdLine.Length < 3)
                    {
                        Console.WriteLine("Invalid command. Try again.");
                        break;
                    }
                    //если не верное имя
                    if (!Enum.TryParse(cmdLine[1], out ServiceNames name))
                    {
                        Console.WriteLine("Wrong service name. Try again.");
                        break;
                    }
                    //получаем сервис
                    var s = Management.Services[name];

                    switch (cmdLine[2])
                    {
                        case "start":
                            //если запуск сервиса то запускаем
                            ServiceManagement.LogInfo($"Enter '{cmdLine[0]} {cmdLine[1]} {cmdLine[2]}' command");
                            s.Start();
                            ServiceManagement.LogSplit();
                            break;
                        case "stop":
                            //если остановка сервиса
                            ServiceManagement.LogInfo($"Enter '{cmdLine[0]} {cmdLine[1]} {cmdLine[2]}' command");
                            s.Stop();
                            ServiceManagement.LogSplit();
                            break;
                        case "ping":
                            //если проверка ping
                            s.Ping();
                            break;
                        default:
                            //если неверная команда
                            Console.WriteLine("Invalid service command. Try again.");
                            break;
                    }
                    break;
                default:
                    //если команда не распознана
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Wrong command! Enter '?' or 'help'.");
                    Console.ResetColor();
                    break;
            }
        }
    }
    
}


internal static class WriteTable
{
    //длинна колонки имя
    private const int NameLength = 20;
    //длинна колонки ip
    private const int IpLength = 16;
    //длинна колонки пути
    private const int PathLength = 50;
    //длинна колонки автозапуска
    private const int AutoStartLength = 12;
    /// <summary>
    /// Выводим шапку
    /// </summary>
    public static void WriteHeader()
    {
        const ConsoleColor color = ConsoleColor.DarkCyan;

        WriteFill();
        Console.Write("|        ");
        Console.ForegroundColor = color;
        Console.Write("Name");
        Console.ResetColor();
        Console.Write("        |");
        Console.Write($"{new string(' ', PathLength / 2 - 2)}");
        Console.ForegroundColor = color;
        Console.Write("Path");
        Console.ResetColor();
        Console.Write($"{new string(' ', PathLength / 2 - 2)}|");

        Console.Write($"{new string(' ', IpLength / 2 - 1)}");
        Console.ForegroundColor = color;
        Console.Write("IP");
        Console.ResetColor();
        Console.Write($"{new string(' ', IpLength / 2 - 1)}");

        Console.Write("| ");
        Console.ForegroundColor = color;
        Console.Write("Auto Start");
        Console.ResetColor();
        Console.WriteLine(" |");
        WriteFill();
    }
    /// <summary>
    /// Выводим Линию
    /// </summary>
    public static void WriteFill()
    {
        Console.WriteLine($"|{new string('-', NameLength)}|{new string('-', PathLength)}|{new string('-', IpLength)}|{new string('-', AutoStartLength)}|");
    }
    /// <summary>
    /// Выводим строку
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="path">Путь</param>
    /// <param name="ip">IP</param>
    /// <param name="autoStart">Автозапуск</param>
    public static void WriteRow(string name, string path, string ip, string autoStart)
    {
        var spaces = NameLength - name.Length - 1;
        Console.Write($"| {name}{new string(' ', spaces)}");

        if (File.Exists(path))
        {
            var blocks = path.Split('\\').ToList();

            var replaceIndex = -1;
            while(true)
            {
                var sum = blocks.Sum(s => s.Length + 1);

                if(sum > PathLength - 2)
                {
                    switch(replaceIndex)
                    {
                        case -1:
                        {
                            replaceIndex = blocks.Count - 3;
                            if(replaceIndex < 0) replaceIndex = 0;
                            blocks[replaceIndex] = "...";
                            continue;
                        }
                        case > 0:
                            blocks.RemoveAt(replaceIndex - 1);
                            replaceIndex--;
                            break;
                        default:
                            blocks.RemoveAt(replaceIndex);
                            break;
                    }

                    if(blocks.Count < 2)
                    {
                        break;
                    }
                    continue;
                }
                break;
            }

            path = string.Join('\\', blocks);

            spaces = PathLength - path.Length - 1;
            if(spaces < 0) spaces = 0;
            Console.Write($"| {path}{new string(' ', spaces)}");
        }
        else
        {
            path = "Not found...";

            spaces = PathLength - path.Length - 1;
            if(spaces < 0) spaces = 0;
            Console.Write("| ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"{path}{new string(' ', spaces)}");
            Console.ResetColor();
        }
        
        spaces = IpLength - ip.Length - 1;
        Console.Write($"| {ip}{new string(' ', spaces)}");

        spaces = AutoStartLength - autoStart.Length - 1;
        Console.WriteLine($"| {autoStart}{new string(' ', spaces)}|");
    }
}
