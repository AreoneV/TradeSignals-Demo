﻿using System.Net;
using MarketInfo;
using Protocol;

// ReSharper disable StringLiteralTypo
namespace MarketData;

internal class Service(IPEndPoint controller, string myIp, int myPort)
{
    private const string LogFileName = "logs.txt";

    private readonly StreamWriter writer = new StreamWriter(LogFileName, true);

    private readonly Dictionary<string, Symbol> symbols = new();

    private readonly Dictionary<string, List<Bar>> future = new();

    private int maxLineLength = 1;

    private readonly Server server = new(myIp, myPort, 10);
    private readonly Client controllerConnection = new(controller);

    private readonly EventWaitHandle waitClosing = new(false, EventResetMode.AutoReset);

    public void Run()
    {
        LogInfo("Starting...");
        //соединение с контроллером


        //
        LogInfo("Connection to controller is established!");
        try
        {
            //загрузка данных, история и будущее
            Load("GBPUSD", 5, "Data\\gbpusd_history.bin", "Data\\gbpusd_future.bin");
            Load("EURUSD", 5, "Data\\eurusd_history.bin", "Data\\eurusd_future.bin");
            LogInfo("Loading history completed successfully!");
        }
        catch(Exception ex)
        {
            LogError($"Error loading history: {ex}.\n");
            //отправить контроллеру
            LogSplit();
            return;
        }

        server.UserConnected += ServerOnUserConnected;
        server.ServerStarted += ServerOnServerStarted;

        try
        {
            server.Start(false);
        }
        catch (Exception e)
        {
            LogError($"Error starting server: {e}.\n");
            //отправить контроллеру
            LogSplit();
            return;
        }
        waitClosing.WaitOne();
        waitClosing.Dispose();
        LogSplit();
        writer.Close();
    }

    

    public void Close(string reason)
    {
        LogWarning($"Server is stopping... Reason: {reason}");

        server.Stop();
        //отправляем на контроллер
        //закрываем контроллер

        LogWarning("Server was stopped!");
        waitClosing.Set();
    }

    //Загрузка и добавление одного символа
    private void Load(string name, int digits, string historyFile, string futureFile)
    {
        var fs = File.OpenRead(historyFile);
        var br = new BinaryReader(fs);

        Symbol symbol = new Symbol(name, digits);

        var hst = symbol.Histories[TimeFrame.M1];

        while (fs.Position < fs.Length)
        {
            hst.Add(Bar.Create(br));
        }

        foreach (var timeFrame in Enum.GetValues<TimeFrame>().OrderBy(x => x))
        {
            if(timeFrame == TimeFrame.M1) continue;

            symbol.Histories[timeFrame].Add(hst.Bars);
            hst = symbol.Histories[timeFrame];
        }

        symbols.Add(name, symbol);

        br.Close();
        fs.Dispose();

        fs = File.OpenRead(futureFile);
        br = new BinaryReader(fs);

        var list = new List<Bar>((int)fs.Length / Bar.BarLength);

        while(fs.Position < fs.Length)
        {
            list.Add(Bar.Create(br));
        }

        future.Add(name, list);

        br.Close();
        fs.Dispose();
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
    }

    private void LogSplit()
    {
        writer.WriteLine(new string('_', maxLineLength));
        writer.WriteLine();
        writer.Flush();
    }


    private void ServerOnServerStarted(Server srv)
    {
        LogInfo("Server is running...");
    }
    private void ServerOnUserConnected(Server srv, Client client)
    {
        client.ReceivedRequest += ClientOnReceivedRequest;
        client.ClientDisconnected += ClientOnClientDisconnected;
    }
    private void ClientOnClientDisconnected(Client client)
    {

    }
    private void ClientOnReceivedRequest(Client client, byte[] message)
    {

    }
}