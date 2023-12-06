using Services;

namespace Controller;

internal class Program
{
    private static readonly ServiceManagement Management = new();


    private static void Main()
    {
        Console.Title = "Service controller";
        Console.Write("Loading...");
        Management.Load();
        Console.WriteLine();

        WriteTable.WriteHeader();

        foreach ((ServiceNames _, ServiceObject value) in Management.Services)
        {
            WriteTable.WriteRow(value.Name.ToString(), value.FullPath, value.Ip, value.AutoStart.ToString());
            WriteTable.WriteFill();
        }

        Console.WriteLine();
        Console.WriteLine("Status: stopped");

        Console.WriteLine("If you need help enter '?' or 'help'");
        
        WhileCommand();
    }


    
    private static void WhileCommand()
    {
        while (true)
        {
            Console.Write("Enter command:");
            var cmdLine = Console.ReadLine()?.Split(' ');
            if (cmdLine == null)
            {
                continue;
            }

            (int Left, int Top) pos = Console.GetCursorPosition();

            Console.SetCursorPosition(0, pos.Top - 1);
            Console.WriteLine(new string(' ', 100));
            Console.SetCursorPosition(0, pos.Top - 1);

            var cmd = cmdLine[0];
            switch(cmd)
            {
                case "exit":
                    Management.Stop();
                    return;
                case "start":
                    Management.Start();
                    break;
                case "help":
                case "?":
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
                    Console.Write($"{empty}name");
                    Console.ResetColor();
                    Console.WriteLine(" - Show service names.");
                    Console.WriteLine();

                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write($"{empty}log");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write(" [count logs]");
                    Console.ResetColor();
                    Console.WriteLine(" - Show last logs. Example: log 20 - Show last 20 logs. If not enter number, show last 10 logs.");
                    Console.WriteLine();

                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write($"{empty}change ");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write(" [name] [property] [new value]");
                    Console.ResetColor();
                    Console.WriteLine(" - Changing service property.");
                    Console.WriteLine($"{empty}Properties: Ip = '-n', Path = '-p', Auto Start = '-as'.");
                    Console.WriteLine($"{empty}Path writes \"C:\\Folder\\File.exe\"; IP writes 127.0.0.1; Boolean writes true, false");
                    Console.WriteLine($"{empty}Example: change MarketData -n 192.168.2.21 (change ip address);");
                    Console.WriteLine($"{empty}Example: change AI -p \"C:\\MainFolder\\Folder2\\Service\\AI.exe (change path to executable file)\"");
                    Console.WriteLine($"{empty}Example: change DB -as false (change auto start)");
                    Console.WriteLine();

                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write($"{empty}monitor");
                    Console.ResetColor();
                    Console.WriteLine(" - Monitoring of services info.");
                    Console.WriteLine();

                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write($"{empty}service ");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write("[name]  [command]");
                    Console.ResetColor();
                    Console.WriteLine(" - Service management. Stopping, Starting and Information");
                    Console.WriteLine($"{empty}stop - stopping the service; start - starting the service");
                    Console.WriteLine($"{empty}ping - check connection ping; info - resources information");
                    Console.WriteLine();

                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write($"{empty}update ");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write("[name] [new *.exe file]");
                    Console.ResetColor();
                    Console.WriteLine(" - Service updating.");
                    Console.WriteLine($"{empty}The first stopping old service and then move new service and starting.");
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
                default:
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
    private const int NameLength = 20;
    private const int IpLength = 16;
    private const int PathLength = 50;
    private const int AutoStartLength = 12;
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
    public static void WriteFill()
    {
        Console.WriteLine($"|{new string('-', NameLength)}|{new string('-', PathLength)}|{new string('-', IpLength)}|{new string('-', AutoStartLength)}|");
    }
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
