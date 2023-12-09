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
                case "stop":
                    Management.Stop();
                    break;
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
                    Console.Write($"{empty}log");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write(" [count logs]");
                    Console.ResetColor();
                    Console.WriteLine(" - Show last logs. Example: log 20 - Show last 20 logs. If not enter number, show last 10 logs.");
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
                    Management.Clear();
                    break;
                case "service":

                    if (!Management.IsStarted)
                    {
                        Console.WriteLine("Controller is stopped.");
                        break;
                    }

                    if (cmdLine.Length < 3)
                    {
                        Console.WriteLine("Invalid command. Try again.");
                        break;
                    }

                    if (!Enum.TryParse(cmdLine[1], out ServiceNames name))
                    {
                        Console.WriteLine("Wrong service name. Try again.");
                        break;
                    }

                    var s = Management.Services[name];

                    switch (cmdLine[2])
                    {
                        case "start":
                            try
                            {
                                s.CommonStart();
                            }
                            catch(Exception ex)
                            {
                                Console.WriteLine($"Error starting: {ex.Message}");
                                //ignored
                            }
                            finally
                            {
                                Management.Update();
                            }
                            break;
                        case "stop":
                            s.Stop();
                            break;
                        case "ping":
                            s.Ping();
                            break;
                        default:
                            Console.WriteLine("Invalid service command. Try again.");
                            break;
                    }
                    break;
                case "log":
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
