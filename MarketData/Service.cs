using MarketInfo;
using Protocol;
using Protocol.MarketData;

// ReSharper disable StringLiteralTypo
namespace MarketData;

internal class Service(string myIp, int myPort)
{
    private const string LogFileName = "logs.txt";
    private readonly StreamWriter writer = new(LogFileName, true);

    private readonly Dictionary<string, Symbol> symbols = [];

    private readonly Dictionary<string, (List<Bar> list, int index)> future = [];

    private int maxLineLength = 1;

    private readonly Server server = new(myIp, myPort, 10);

    private bool isRunning = true;
    private const int sleepImitation = 500; 

    public ExitCode Run()
    {
        LogInfo("Starting...");
        
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
            LogSplit();
            return ExitCode.ErrorStarting;
        }

        server.UserConnected += ServerOnUserConnected;
        server.ServerStarted += ServerOnServerStarted;

        try
        {
            server.Start(false);
        }
        catch (Exception e)
        {
            LogError($"Error running server: {e}.\n");
            LogSplit();
            return ExitCode.ErrorStarting;
        }

        LogWarning("Server was stopped!");
        LogSplit();
        writer.Close();

        isRunning = false;

        return ExitCode.Ok;
    }

    

    public void Close(string reason)
    {
        LogWarning($"Server is stopping... Reason: {reason}");
        server.Stop();
    }

    //Загрузка и добавление одного символа
    private void Load(string name, int digits, string historyFile, string futureFile)
    {
        var fs = File.OpenRead(historyFile);
        var br = new BinaryReader(fs);

        Symbol symbol = new(name, digits);

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

        future.Add(name, (list, 0));

        br.Close();
        fs.Dispose();
    }

    private void ImitationWorking()
    {
        isRunning = true;
        Task.Run(() =>
        {

            while(isRunning)
            {
                try
                {
                    foreach(var item in future)
                    {
                        if(item.Value.index >= item.Value.list.Count) return;
                        var symbol = item.Key;

                        var (list, index) = future[symbol];
                        var bar = list[index];
                        index++;

                        future[symbol] = (list, index);

                        var sym = symbols[symbol];
                        foreach (var tf in Enum.GetValues<TimeFrame>())
                        {
                            var h = sym.Histories[tf];
                            h.Add(bar);
                        }
                    }
                    Thread.Sleep(sleepImitation);
                }
                catch(Exception ex)
                {
                    LogError($"Error imitation working: {ex.Message}");
                    return;
                }
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
        ImitationWorking();
    }
    private void ServerOnUserConnected(Server srv, Client client)
    {
        client.ReceivedRequest += ClientOnReceivedRequest;
        client.ClientDisconnected += ClientOnClientDisconnected;
    }
    private void ClientOnClientDisconnected(Client client)
    {
        client.ReceivedRequest -= ClientOnReceivedRequest;
        client.ClientDisconnected -= ClientOnClientDisconnected;
    }
    private void ClientOnReceivedRequest(Client client, byte[] message)
    {
        using var ms = new MemoryStream(message);
        var reader = new BinaryReader(ms);

        using var answer = new MemoryStream();
        var writer = new BinaryWriter(answer);
        try
        {

            var common = (CommonCommand)reader.ReadInt32();
            switch(common)
            {
                case CommonCommand.Shutdown:
                    Close("Received shutdown command!");
                    break;
                case CommonCommand.SpecialCommand:
                    var specCommand = (MarketDataCommand)reader.ReadInt32();
                    ProcessingCommand(specCommand, reader, writer);
                    client.SendAnswer(answer.ToArray());
                    break;
                case CommonCommand.Logs:
                    if(!File.Exists(LogFileName))
                    {
                        writer.Write("There isn't logs");
                        client.SendAnswer(answer.ToArray());
                        return;
                    }
                    writer.Write(File.ReadAllText(LogFileName));
                    client.SendAnswer(answer.ToArray());
                    return;
                default:
                    client.SendAnswer(message);
                    return;
            }
        }
        catch(Exception ex)
        {
            LogError($"Error processing command: {ex.Message}");
            writer.Write(ex.ToString());
            client.SendAnswer(answer.ToArray());
        }
    }
    private void ProcessingCommand(MarketDataCommand command, BinaryReader reader, BinaryWriter writer)
    {
        string symbol;
        TimeFrame tf;
        switch(command)
        {
            case MarketDataCommand.GetSymbolNames:
                writer.Write(symbols.Count);
                foreach(var symbolName in symbols.Keys)
                {
                    writer.Write(symbolName);
                }
                break;
            case MarketDataCommand.GetLasBar:
                symbol = reader.ReadString();
                tf = (TimeFrame)reader.ReadInt32();
                try
                {
                    symbols[symbol].Histories[tf].LastBar.Save(writer);
                }
                catch
                {
                    (new Bar()).Save(writer);
                }
                break;
            case MarketDataCommand.GetExtremeDate:
                symbol = reader.ReadString();
                tf = (TimeFrame)reader.ReadInt32();
                try
                {
                    writer.Write(symbols[symbol].Histories[tf].FirstBar.Date.Ticks);
                    writer.Write(symbols[symbol].Histories[tf].LastBar.Date.Ticks);
                }
                catch
                {
                    writer.Write(new DateTime().Ticks);
                    writer.Write(new DateTime().Ticks);
                }
                break;
            case MarketDataCommand.GetBars:
                symbol = reader.ReadString();
                tf = (TimeFrame)reader.ReadInt32();
                int count = reader.ReadInt32();
                try
                {
                    var h = symbols[symbol].Histories[tf];
                    var list = h.Bars.TakeLast(count);
                    writer.Write(list.Count());
                    foreach (var bar in list)
                    {
                        bar.Save(writer);
                    }
                }
                catch
                {
                    (new Bar()).Save(writer);
                }
                break;
            default:
                writer.Write("Error command");
                break;
        }
    }
}