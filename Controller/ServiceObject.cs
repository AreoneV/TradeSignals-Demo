﻿using System.Diagnostics;
using Protocol;
using Services;

namespace Controller;

public class ServiceObject
{
    private bool isStartedListen;

    public ServiceObject(ServiceNames name, string ip, string fullPath)
    {
        Name = name;
        Ip = ip;
        FullPath = fullPath;
        EventServiceStatusChanged += OnEventServiceStatusChanged;
    }

    

    public ServiceNames Name { get; }
    public string FullPath { get; set; }
    public string Ip { get; set; }
    public bool AutoStart { get; set; } = true;
    public int Port { get; private set; }

    public int ExitCode { get; private set; }

    public Process Process { get; set; }
    public Client Client { get; set; }


    public ServiceStatus Status { get; private set; } = ServiceStatus.NotWorking;


    public delegate void ServiceStatusDelegate(ServiceObject service, ServiceStatus status);

    public event ServiceStatusDelegate EventServiceStatusChanged;


    public void StartProcess(int port)
    {
        EventServiceStatusChanged?.Invoke(this, ServiceStatus.Starting);

        Port = port;

        Process = Process.Start(FullPath, $"{Ip} {Port}");

        if (Process == null || (Process.WaitForExit(1000) && Process.HasExited))
        {
            EventServiceStatusChanged?.Invoke(this, ServiceStatus.Error);
            throw new Exception("Service didn't start. Error starting process");
        }
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
            EventServiceStatusChanged?.Invoke(this, ServiceStatus.Error);
            throw;
        }
        Client.ClientDisconnected += ClientOnClientDisconnected;
    }
    public void StartListenInfo()
    {
        isStartedListen = true;
        EventServiceStatusChanged?.Invoke(this, ServiceStatus.Ok);
        Task.Run(() =>
        {
            while (isStartedListen)
            {
                if (Process.HasExited)
                {
                    Stop();
                }


                if(!Client.IsConnected)
                {
                    Stop();
                    break;
                }

                try
                {
                    Client.Request(new byte[100], 1000);
                    if(Status !=  ServiceStatus.Ok)
                        EventServiceStatusChanged?.Invoke(this, ServiceStatus.Ok);
                }
                catch(TimeoutException)
                {
                    EventServiceStatusChanged?.Invoke(this, ServiceStatus.NotResponding);
                }
                catch
                {
                    Stop();
                }
            }
        });
    }


    public void Stop()
    {
        EventServiceStatusChanged?.Invoke(this, ServiceStatus.Stopping);
        isStartedListen = false;
        if (Client is { IsConnected: true })
        {
            try
            {
                Client.Request(new []{ (byte)128 }, 1000);
            }
            finally
            {
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
        ExitCode = Process.ExitCode;
        Client = null;
        Process = null;
        EventServiceStatusChanged?.Invoke(this, ServiceStatus.NotWorking);
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
                    var a = Client.Request(new byte[(i + 1) * 100], 100);
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
    private void ClientOnClientDisconnected(Client client)
    {
        Stop();
    }
    private void OnEventServiceStatusChanged(ServiceObject service, ServiceStatus status)
    {
        Status = status;
    }
}