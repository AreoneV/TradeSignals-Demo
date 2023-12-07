using System.Diagnostics;
using Protocol;

namespace Controller;

public class ServiceObject(ServiceNames name, string ip, string fullPath)
{
    public ServiceNames Name { get; } = name;
    public string FullPath { get; set; } = fullPath;
    public string Ip { get; set; } = ip;
    public bool AutoStart { get; set; } = true;
    public int Port { get; set; }

    public Process Process { get; set; }
    public Client Client { get; set; }




    
    public void StartProcess(int port)
    {
        Port = port;

        Process = Process.Start(FullPath, $"{Ip} {Port}");

        if (Process == null || (Process.WaitForExit(1000) && Process.HasExited))
        {
            throw new Exception("Service didn't start. Error starting process");
        }
    }
    public void StartConnect()
    {
        Client = new Client(Ip, Port);
        Client.Connect();
        Client.ClientDisconnected += ClientOnClientDisconnected;
    }
    public void StartListenInfo()
    {
        isStartedListen = true;
        Task.Run(() =>
        {
            while (isStartedListen)
            {
                if (Process.HasExited)
                {
                    //if process closed
                }


                if(!Client.IsConnected)
                {
                    //stop and event
                    break;
                }

                try
                {
                    var a = Client.Request(new byte[100], 1000);
                }
                catch(TimeoutException)
                {
                    //not responding
                }
                catch
                {
                    //close connection
                }
            }

            Console.WriteLine();
            Console.WriteLine($"  Process exit code          : {Process.ExitCode}");
        });
    }


    public void Stop()
    {
        isStartedListen = false;
        if (Client is { IsConnected: true })
        {
            Client.Close();
            Client = null;
        }

        // ReSharper disable once InvertIf
        if(Process is { HasExited: false})
        {
            Process.CloseMainWindow();
            Process.Close();
            Process = null;
        }
    }



}