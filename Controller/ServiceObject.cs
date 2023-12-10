﻿using System.Diagnostics;
using Protocol;
using Services;

namespace Controller;

public class ServiceObject(ServiceNames name, string ip, string fullPath)
{
    private bool isStartedListen;

    private bool isNormalStopping;

    private bool checkPing;


    public ServiceNames Name { get; } = name;
    public string FullPath { get; set; } = fullPath;
    public string Ip { get; set; } = ip;
    public bool AutoStart { get; set; } = true;
    public int Port { get; private set; }

    public int ExitCode { get; private set; }

    public Process Process { get; set; }
    public Client Client { get; set; }


    public bool IsRunning { get; set; }


    public void CommonStart()
    {

        if(!File.Exists(FullPath))
        {
            throw new FileNotFoundException();
        }

        StartProcess(Port);
        StartConnect();
        StartListenInfo();
    }


    public void StartProcess(int port)
    {

        var localByName = Process.GetProcessesByName($"{Name}");
        foreach(Process p in localByName)
        {
            p.Kill();
        }

        Port = port;

        Process = Process.Start(FullPath, $"{Ip} {Port}");

        if (Process != null && (!Process.WaitForExit(1000) || !Process.HasExited)) return;

        throw new Exception("Service didn't start. Error starting process");
    }
    public void StartConnect()
    {
        Client = new Client(Ip, Port);
        try
        {
            Client.Connect();
        }
        catch
        {
            throw;
        }
    }
    public void StartListenInfo()
    {
        if(isStartedListen) { return; }
        isStartedListen = true;
        Task.Run(() =>
        {
            while (isStartedListen)
            {
                if(checkPing)
                {
                    Thread.Sleep(100);
                    continue;
                }

                if (Process.HasExited)
                {
                    CriticalStopping();
                }


                if(!Client.IsConnected)
                {
                    CriticalStopping();
                    break;
                }

                try
                {
                    lock (Client)
                    {
                        Client.Request(new byte[100], 1000);
                    }
                }
                catch(TimeoutException)
                {
                }
                catch
                {
                    CriticalStopping();
                }
            }
        });
    }

    
    public void Stop()
    {
        isNormalStopping = true;
        isStartedListen = false;
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
        isNormalStopping = false;
    }

    public void CriticalStopping()
    {
        if(isNormalStopping) { return; }
        isStartedListen = false;
        if(Client is { IsConnected: true })
        {
            try
            {
                Client.Request(new[] { (byte)128 }, 1000);
            }
            finally
            {
                Client.Close();
            }
        }

        // ReSharper disable once InvertIf
        if(Process is { HasExited: false })
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
        Thread.Sleep(1000);
        if (!AutoStart) return;

        try
        {
            CommonStart();
        }
        catch
        {
        }
    }

    public void SendUpdate(byte[] data)
    {
        try
        {
            lock(Client)
            {
                Client.Request(data);
            }
        }
        catch(TimeoutException)
        {
        }
        catch
        {
            CriticalStopping();
        }
    }

    public void Ping()
    {
        if(Client is not { IsConnected: true })
        {
            Console.WriteLine("No connection!");
            return;
        }

        checkPing = true;

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

        checkPing = false;
    }
}