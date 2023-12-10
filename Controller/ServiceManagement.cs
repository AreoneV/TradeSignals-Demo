using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using Services;

namespace Controller;

public class ServiceManagement
{
    private const string SettingFile = "services.txt";
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

    public void Start()
    {
        if(IsStarted) return;

        Console.Clear();
        const int maxLen = 30;

        Console.Write("Status: ");

        Console.ForegroundColor = ConsoleColor.Yellow;

        Console.WriteLine("Starting...");
        Console.ResetColor();

        var indent = new string(' ', 4);

        foreach(var service in services)
        {
            Console.Write($"{indent}{service.Value.Name}{new string('.', maxLen - service.Value.Name.ToString().Length)}");
            var stat = service.Value.Start(GetFreePort()) ? "Running" : "Stopped";
            Console.ForegroundColor = service.Value.IsRunning ? ConsoleColor.Green : ConsoleColor.Yellow;
            Console.Write($"{stat}");
            Console.ResetColor();
        }

        IsStarted = true;

        Checking();

        WriteInfo();
    }
    public void Stop()
    {
        if (!IsStarted)
        {
            return;
        }
        IsStarted = false;

        foreach (var service in services)
        {
            service.Value.Stop();
        }

        Update();
    }

    public void SendUpdate()
    {
        if (!IsStarted) { return;}

        using var ms = new MemoryStream();
        var w = new BinaryWriter(ms);
        foreach ((ServiceNames key, ServiceObject value) in services)
        {
            w.Write((int)key);
            w.Write(value.Ip);
            w.Write(value.Port);
            w.Write(value.Status == ServiceStatus.Ok);
        }
        var data = ms.ToArray();
        w.Close();

        foreach((ServiceNames _, ServiceObject value) in services)
        {
            value.SendUpdate(data);
        }
    }
    public void Clear()
    {
        Console.Clear();
        WriteInfo();
    }

    public void Update()
    {
        var pos = Console.GetCursorPosition();
        Console.SetCursorPosition(0, 0);

        WriteInfo();
        Console.SetCursorPosition(pos.Left, pos.Top);
    }
    public void WriteInfo()
    {
        const int maxLen = 30;

        Console.Write("Status: ");

        Console.ForegroundColor = IsStarted ? ConsoleColor.Green : ConsoleColor.Red;

        var status = IsStarted ? "Running" : "Stopped";
        Console.WriteLine(status);
        Console.ResetColor();

        var indent = new string(' ', 4);

        foreach (var service in services)
        {
            Console.Write($"{indent}{service.Value.Name}{new string('.', maxLen - service.Value.Name.ToString().Length)}");
            var stat = service.Value.IsRunning ? "Running" : "Stopped";
            Console.ForegroundColor = service.Value.IsRunning ? ConsoleColor.Green : ConsoleColor.Yellow;
            Console.Write($"{stat}");
            Console.ResetColor();
        }
    }

    public void Load()
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

        using StreamReader r = new StreamReader(SettingFile);

        while (!r.EndOfStream)
        {
            var line = r.ReadLine()?.Trim();
            if (line == null) continue;
            if(line.StartsWith("#")) continue;

            var args = line.Split(' ');

            if(args.Length != 4) continue;

            if (!Enum.TryParse(typeof(ServiceNames), args[0], out object result)) continue;

            var name = (ServiceNames)result;
            var s = Services[name];
            s.Ip = args[2];
            s.FullPath = args[1];
            s.AutoStart = !bool.TryParse(args[3], out bool auto) || auto;
        }
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Ok");
        Console.ResetColor();
    }


    private void Checking()
    {
        Task.Run(() =>
        {
            while (IsStarted)
            {
                bool changed = false;
                foreach (var o in services.Where(o => o.Value.IsRunning && !o.Value.CheckRunning() && o.Value.AutoStart))
                {
                    o.Value.Stop();
                    if (o.Value.Start(GetFreePort())) continue;
                    o.Value.IsRunning = false;
                    changed = true;
                }

                if (changed)
                {
                    Update();
                }
                Thread.Sleep(1000);
            }
        });
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