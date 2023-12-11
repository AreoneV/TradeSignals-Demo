using System.Collections.ObjectModel;
using Services;

namespace Controller;

public class ServiceManagement
{
    private const string SettingFile = "services.txt";
    private const string LogFile = "logs.txt";

    private readonly Dictionary<ServiceNames, ServiceObject> services;

    private static readonly StreamWriter Writer = new(LogFile, true);
    private static int _maxLineLength = 1;


    public ServiceManagement()
    {
        services = new Dictionary<ServiceNames, ServiceObject>();
        Services = services.AsReadOnly();
    }

    public ReadOnlyDictionary<ServiceNames, ServiceObject> Services { get; }
    public bool IsStarted { get; private set; }

    public void Start()
    {
        if(IsStarted) return;

        const int maxLen = 30;

        Console.Write("Status: ");

        Console.ForegroundColor = ConsoleColor.Yellow;

        Console.WriteLine("Wait...");
        Console.ResetColor();

        var indent = new string(' ', 4);

        foreach(var service in services)
        {
            Console.Write($"{indent}{service.Value.Name}{new string('.', maxLen - service.Value.Name.ToString().Length)}");
            service.Value.Start();
            var stat = service.Value.IsRunning ? "Running" : "Stopped";
            Console.ForegroundColor = service.Value.IsRunning ? ConsoleColor.Green : ConsoleColor.Yellow;
            Console.WriteLine($"{stat}");
            Console.ResetColor();
        }

        IsStarted = true;

        CheckRunning();

        Console.Clear();
        WriteInfo();
    }
    public void Stop()
    {
        if(!IsStarted)
        {
            return;
        }
        IsStarted = false;

        foreach(var service in services)
        {
            service.Value.Stop();
        }
        Console.Clear();
        WriteInfo();
    }

    public void SendUpdate()
    {
        if(!IsStarted) { return; }

        using var ms = new MemoryStream();
        var w = new BinaryWriter(ms);
        foreach((ServiceNames key, ServiceObject value) in services)
        {
            w.Write((int)key);
            w.Write(value.Ip);
            w.Write(value.Port);
            w.Write(value.IsRunning);
        }
        var data = ms.ToArray();
        w.Close();

        foreach((ServiceNames _, ServiceObject value) in services)
        {
            if(!value.IsRunning) continue;
            try
            {
                lock(value.Client)
                {
                    value.Client.Request(data, 1000);
                }
            }
            catch
            {
                // ignored
            }

        }
    }
    public void Clear()
    {
        Console.Clear();
        WriteInfo();
    }

    private void CheckRunning()
    {
        Task.Run(() =>
        {
            while(IsStarted)
            {
                foreach(var o in services.Where(o => o.Value.IsRunning))
                {
                    try
                    {
                        lock(o.Value.Client)
                        {
                            o.Value.Client.Request(new byte[100], 1000);
                        }
                    }
                    catch
                    {
                        // not responding
                    }
                }
                Thread.Sleep(1000);
            }

        });
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

        foreach(var service in services)
        {
            Console.Write($"{indent}{service.Value.Name}{new string('.', maxLen - service.Value.Name.ToString().Length)}");
            var stat = service.Value.IsRunning ? "Running" : "Stopped";
            Console.ForegroundColor = service.Value.IsRunning ? ConsoleColor.Green : ConsoleColor.Yellow;
            Console.WriteLine($"{stat}");
            Console.ResetColor();
        }
    }

    public void Load()
    {
        foreach(var value in Enum.GetValues<ServiceNames>())
        {
            services.Add(value, new ServiceObject(value, "127.0.0.1", Directory.GetCurrentDirectory() + $"\\{value}.exe"));
            services[value].EventRunningStatusChanged += OnEventRunningStatusChanged;
        }
        if(!File.Exists(SettingFile))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Ok");
            Console.ResetColor();
            return;
        }

        using StreamReader r = new StreamReader(SettingFile);

        while(!r.EndOfStream)
        {
            var line = r.ReadLine()?.Trim();
            if(line == null) continue;
            if(line.StartsWith("#")) continue;

            var args = line.Split(' ');

            if(args.Length != 4) continue;

            if(!Enum.TryParse(typeof(ServiceNames), args[0], out object result)) continue;

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

    private void OnEventRunningStatusChanged(ServiceObject service, bool status)
    {
        if(!IsStarted) return;

        Console.Clear();
        WriteInfo();

        SendUpdate();
    }


    public static void LogInfo(string msg)
    {
        var m = $"{DateTime.Now:G} | Info\t| {msg}";
        Writer.WriteLine(m);
        Writer.Flush();
        if(m.Length > _maxLineLength)
        {
            _maxLineLength = m.Length;
        }
    }
    public static void LogWarning(string msg)
    {
        var m = $"{DateTime.Now:G} | Warning\t| {msg}";
        Writer.WriteLine(m);
        Writer.Flush();
        if(m.Length > _maxLineLength)
        {
            _maxLineLength = m.Length;
        }
    }
    public static void LogError(string msg)
    {
        var m = $"{DateTime.Now:G} | Error\t| {msg}";
        Writer.WriteLine(m);
        Writer.Flush();
        if(m.Length > _maxLineLength)
        {
            _maxLineLength = m.Length;
        }
    }
    public static void LogSplit()
    {
        Writer.WriteLine(new string('_', _maxLineLength));
        Writer.WriteLine();
        Writer.Flush();
    }
    public static void LogClose()
    {
        Writer.Flush();
        Writer.Close();
    }

}