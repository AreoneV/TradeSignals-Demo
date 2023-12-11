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

    public int ExitCode { get; private set; }

    public Process Process { get; set; }
    public Client Client { get; set; }


    public bool IsRunning { get; set; }


    public delegate void RunningDelegate(ServiceObject service, bool status);
    public event RunningDelegate EventRunningStatusChanged;


    public void Start()
    {
        if(IsRunning) { return; }

        if(!File.Exists(FullPath))
        {
            return;
        }

        var localByName = Process.GetProcessesByName($"{Name}");
        foreach(Process p in localByName)
        {
            p.Kill();
        }

        Port = GetFreePort();

        Process = Process.Start(FullPath, $"{Ip} {Port}")!;

        Client = new Client(Ip, Port);

        int attempts = 5;

        do
        {
            try
            {
                Client.Connect();
                Client.ClientDisconnected += ClientOnClientDisconnected;
                break;
            }
            catch
            {
                if (Process.HasExited)
                {
                    Stop();
                    return;
                }
                attempts--;
            }
            Thread.Sleep(1000);
        } while (attempts > 0);


        if (!Client.IsConnected)
        {
            Stop();

            return;
        }

        IsRunning = true;
        EventRunningStatusChanged?.Invoke(this, true);
    }
    public void Stop()
    {
        if(!IsRunning) { return; }
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
                }
            }
            catch
            {
                //ignored
            }
            
        }
        ExitCode = Process?.ExitCode ?? ExitCode;
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
            if(tcpConnInfoArray.All(con => con.LocalEndPoint.Port != i)) return i;
        }

        return -1;
    }
    private void ClientOnClientDisconnected(Client client)
    {
        if(AutoStart)
        {
            Stop();
            Start();
            if (IsRunning) return;

            //попытка перезапуска безуспешна
            Stop();
            return;
        }
        Stop();
    }
}