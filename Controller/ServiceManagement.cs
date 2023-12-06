using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using Services;

namespace Controller;

public class ServiceManagement
{
    private const string SettingFile = "services.setting";
    private const string LogFile = "logs.txt";

    private readonly Range portRange = new(41222, 41300);
    private readonly IPGlobalProperties ipGlobalProperties;
    private readonly Dictionary<ServiceNames, ServiceObject> services;

    private readonly StreamWriter writer = new(LogFile, true);
    private readonly List<string> logs = new();
    private int maxLineLength = 1;


    public ServiceManagement()
    {
        services = new Dictionary<ServiceNames, ServiceObject>();
        Services = services.AsReadOnly();
        ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
    }

    public ReadOnlyDictionary<ServiceNames, ServiceObject> Services { get; }
    public bool IsStarted { get; private set; }

    public void Load()
    {
        ReadFile();
    }
    public void Start()
    {
        if(IsStarted) return;

        Console.WriteLine("Starting services:");
        Console.WriteLine("{");

        foreach ((ServiceNames name, ServiceObject value) in services)
        {
            var indent = new string(' ', 4);
            Console.WriteLine($"{indent}{name}:");
            Console.WriteLine($"{indent}" + "{");
            if (!File.Exists(value.FullPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{indent}{indent}Service will not be started. Executable file is not fount!");
                Console.ResetColor();
                Console.WriteLine($"{indent}" + "}");
                continue;
            }

            Console.Write($"{indent}{indent}Starting process...");
            try
            {
                value.StartProcess(GetFreePort());
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Ok");
                Console.ResetColor();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {e.Message}");
                Console.ResetColor();
                continue;
            }

            Console.Write($"{indent}{indent}Starting connection...");
            try
            {
                value.StartConnect();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Ok");
                Console.ResetColor();
            }
            catch(Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {e.Message}");
                Console.ResetColor();
                continue;
            }

            value.StartListenInfo();

            Console.Write($"{indent}{indent}Service has been started!");

        }
        Console.WriteLine("}");
        IsStarted = true;
    }
    public void Stop()
    {

    }


    private void ReadFile()
    {
        foreach(var value in Enum.GetValues<ServiceNames>())
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
                var s = Services[name];
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


    private void LogInfo(string msg)
    {
        var m = $"{DateTime.Now:G} | Info\t| {msg}";
        writer.WriteLine(m);
        writer.Flush();
        if(m.Length > maxLineLength)
        {
            maxLineLength = m.Length;
        }
        logs.Add(msg);
    }
    private void LogWarning(string msg)
    {
        var m = $"{DateTime.Now:G} | Warning\t| {msg}";
        writer.WriteLine(m);
        writer.Flush();
        if(m.Length > maxLineLength)
        {
            maxLineLength = m.Length;
        }
        logs.Add(msg);
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
        logs.Add(msg);
    }
    private void LogSplit()
    {
        writer.WriteLine(new string('_', maxLineLength));
        writer.WriteLine();
        writer.Flush();
    }

    private int GetFreePort()
    {
        var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();
        for (var i = portRange.Start.Value; i < portRange.End.Value; i++)
        {
            if (tcpConnInfoArray.All(con => con.LocalEndPoint.Port != i)) return i;
        }

        return -1;
    }
}