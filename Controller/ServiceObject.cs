using System.Diagnostics;
using Protocol;
using Services;

namespace Controller;

public class ServiceObject(ServiceNames name, string ip, string fullPath)
{
    
    public ServiceNames Name { get; } = name;
    public string FullPath { get; set; } = fullPath;
    public string Ip { get; set; } = ip;
    public bool AutoStart { get; set; } = true;
    public int Port { get; private set; }

    public int ExitCode { get; private set; }

    public Process Process { get; set; }
    public Client Client { get; set; }


    public bool IsRunning { get; set; }


    public bool Start(int port)
    {
        if(IsRunning) { return true; }

        if(!File.Exists(FullPath))
        {
            return false;
        }

        var localByName = Process.GetProcessesByName($"{Name}");
        foreach(Process p in localByName)
        {
            p.Kill();
        }

        Port = port;

        Process = Process.Start(FullPath, $"{Ip} {Port}")!;

        Client = new Client(Ip, Port);

        int attempts = 5;

        do
        {
            try
            {
                Client.Connect();
                break;
            }
            catch
            {
                if (Process.HasExited)
                {
                    Stop();
                    return false;
                }
                attempts--;
            }
            Thread.Sleep(1000);
        } while (attempts > 0);


        if (Client.IsConnected) return true;


        Stop();
        return false;
    }
    public bool CheckRunning()
    {
        
        if(Process is not {HasExited: true} || Client is not { IsConnected: true })
        {
            return false;
        }

        try
        {
            lock(Client)
            {
                Client.Request(new byte[100], 1000);
            }
        }
        catch
        {
            return false;
        }

        return true;
    }
    public void Stop()
    {
        if (Client is { IsConnected: true })
        {
            try
            {
                Client.Request(new []{ (byte)128 }, 1000);
            }
            finally
            {
                Client?.Close();
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
}