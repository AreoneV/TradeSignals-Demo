using Services;

namespace Controller;

internal class Program
{
    private const string SettingFile = "services.setting"; 

    private static Range portRange = new(41222, 41300);

    private static Dictionary<ServiceNames, ServiceObject> services = new();


    static void Main(string[] args)
    {
        Console.Write("Loading...");
        ReadFile();

        //вывести таблицу всех сервисов
        //спросить начать прямо сейчас или хотите изменить настройки

        Console.ReadLine();





        Console.WriteLine("Hello, World!");
    }


    private static void ReadFile()
    {
        foreach (var value in Enum.GetValues<ServiceNames>())
        {
            services.Add(value, new ServiceObject(value, "127.0.0.1", Directory.GetCurrentDirectory() + $"\\{value}.exe"));
        }
        if(!File.Exists(SettingFile))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Ok");
            Console.ResetColor();
            return;
        }

        using FileStream fs = new FileStream(SettingFile, FileMode.Open, FileAccess.Read);
        BinaryReader br = new BinaryReader(fs);

        try
        {
            while(fs.Position < fs.Length)
            {
                var name = (ServiceNames)br.ReadInt32();
                var s = services[name];
                s.Ip = br.ReadString();
                s.FullPath = br.ReadString();
                s.AutoStart = br.ReadBoolean();
            }
        }
        catch
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("...Warning");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error loading setting file!");
            Console.ResetColor();
            return;
        }
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Ok");
        Console.ResetColor();
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
