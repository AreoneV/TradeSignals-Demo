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
}
