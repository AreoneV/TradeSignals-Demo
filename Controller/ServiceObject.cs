using System.Diagnostics;
using System.Net.NetworkInformation;
using Protocol;
using Services;

namespace Controller;

public class ServiceObject(ServiceNames name, string ip, string fullPath)
{
    private readonly Range portRange = new(41222, 41300);

    public ServiceNames Name { get; } = name;
    public string FullPath { get; set; } = fullPath;
    public string Ip { get; set; } = ip;
    public bool AutoStart { get; set; } = true;
    public int Port { get; private set; }


    public Process Process { get; set; }
    public Client Client { get; set; }


    public bool IsRunning { get; set; }


    public delegate void RunningDelegate(ServiceObject service, bool status);
    public event RunningDelegate EventRunningStatusChanged;


    public void Start()
    {
        if(IsRunning) { return; }
        ServiceManagement.LogInfo($"{Name} is starting");
        if(!File.Exists(FullPath))
        {
            ServiceManagement.LogError($"{Name} was not started. Executable file not found.");
            return;
        }

        var localByName = Process.GetProcessesByName($"{Name}");
        if (localByName.Length > 0)
        {
            ServiceManagement.LogWarning($"Found {localByName.Length} the same processes. They will be killed.");
        }
        foreach(Process p in localByName)
        {
            p.Kill();
        }

        Port = GetFreePort();

        Process = Process.Start(FullPath, $"{Ip} {Port}")!;
        ServiceManagement.LogInfo("Process has started");

        Client = new Client(Ip, Port);

        int attempts = 5;

        do
        {
            try
            {
                Client.Connect();
                Client.ClientDisconnected += ClientOnClientDisconnected;
                ServiceManagement.LogInfo("Connection is established");
                break;
            }
            catch
            {
                if (Process.HasExited)
                {
                    ServiceManagement.LogError("Starting failed. Process has exited.");
                    Stop();
                    return;
                }
                attempts--;
                ServiceManagement.LogWarning($"Connection failed. There are still attempts left: {attempts}");
            }
            Thread.Sleep(1000);
        } while (attempts > 0);


        if (!Client.IsConnected)
        {
            ServiceManagement.LogError("Starting failed. Connection has closed.");
            Stop();

            return;
        }

        ServiceManagement.LogInfo($"{Name} has started");
        IsRunning = true;
        EventRunningStatusChanged?.Invoke(this, true);
    }
    public void Stop()
    {
        if(!IsRunning) { return; }
        ServiceManagement.LogInfo($"{Name} is stopping");
        if (Client is { IsConnected: true })
        {
            try
            {
                Client.Request(new []{ (byte)128 }, 1000);
            }
            finally
            {
                Client.ClientDisconnected -= ClientOnClientDisconnected;
                Client.Close();
                ServiceManagement.LogInfo("Connection has closed");
            }
        }

        // ReSharper disable once InvertIf
        if(Process is { HasExited: false})
        {
            try
            {
                if(!Process.WaitForExit(10000))
                {
                    Process.Kill();
                    ServiceManagement.LogWarning("Process was killed");
                }
            }
            catch
            {
                //ignored
            }
        }
        ServiceManagement.LogWarning($"Process code: {Process.ExitCode}");
        Client = null;
        Process = null;
        IsRunning = false;
        EventRunningStatusChanged?.Invoke(this, false);
    }

    public void Ping()
    {
        if(Client is not { IsConnected: true })
        {
            Console.WriteLine("No connection!");
            return;
        }


        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    lock(Client)
                    {
                        Client.Request(new byte[(i + 1) * 100], 100); 
                    }
                    sw.Stop();
                    Console.WriteLine($"Ping {(i + 1) * 100} bytes --> {sw.Elapsed.TotalMilliseconds:F3} ms");
                }
                catch(TimeoutException)
                {
                    Console.WriteLine($"Ping {(i + 1) * 100} bytes --> >= 100 ms");
                }
                catch
                {
                    Console.WriteLine("Error connection!");
                    return;
                }
            }
            
        }
    }

    private int GetFreePort()
    {
        var tcpConnInfoArray = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections();
        for(var i = portRange.Start.Value; i < portRange.End.Value; i++)
        {
            if (tcpConnInfoArray.Any(con => con.LocalEndPoint.Port == i)) continue;

            ServiceManagement.LogInfo($"Got free port: {i}");
            return i;
        }

        return -1;
    }
    private void ClientOnClientDisconnected(Client client)
    {
        if(AutoStart)
        {
            ServiceManagement.LogWarning($"{Name} try to restart");
            Stop();
            Start();
            if (IsRunning)
            {
                ServiceManagement.LogWarning($"Restarting {Name} completed successfully");
                ServiceManagement.LogSplit();
                return;
            }

            ServiceManagement.LogError($"Restarting {Name} failed");
            Stop();
            ServiceManagement.LogSplit();
            return;
        }
        ServiceManagement.LogError("Something went wrong! Service disconnected");
        Stop();
        ServiceManagement.LogSplit();
    }
}