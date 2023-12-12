using Protocol.MarketData;
using System.Diagnostics;

namespace TestMarketData;

[TestClass]
public class TestClient
{
    [TestMethod]
    public void WorkService()
    {
        MarketData.Service service = new MarketData.Service("127.0.0.1", 41222);



        Task.Run(() =>
        {
            if(service.Run() != MarketData.ExitCode.Ok)
            {
                Assert.Fail();
            }
            Trace.WriteLine("Service closed seccessfully");
        });


        var md = new MarketDataFramework("127.0.0.1", 41222);

        var logs = md.GetLogs();
        Assert.IsNotNull(logs);

        Trace.WriteLine("Получено логов:");
        Trace.WriteLine(logs.Length);
        Trace.WriteLine("");

        var symbols = md.GetSymbolNames();
        Trace.WriteLine("Символы:");
        foreach( var symbol in symbols)
        {
            Trace.WriteLine(symbol);
        }

        Trace.WriteLine("");


        var tf = MarketInfo.TimeFrame.M1;
        var ex = md.GetExtremeDate(symbols[0], tf);
        Trace.WriteLine($"Крайние {tf}: {ex.first} - {ex.last}");

        tf = MarketInfo.TimeFrame.M30;
        ex = md.GetExtremeDate(symbols[0], tf);
        Trace.WriteLine($"Крайние {tf}: {ex.first} - {ex.last}");

        tf = MarketInfo.TimeFrame.D1;
        ex = md.GetExtremeDate(symbols[0], tf);
        Trace.WriteLine($"Крайние {tf}: {ex.first} - {ex.last}");

        tf = MarketInfo.TimeFrame.MN1;
        ex = md.GetExtremeDate(symbols[0], tf);
        Trace.WriteLine($"Крайние {tf}: {ex.first} - {ex.last}");

        Trace.WriteLine("");

        Trace.WriteLine($"Ping: {md.Ping(1000000)}");
        Trace.WriteLine($"Ping: {md.Ping(1000000)}");
        Trace.WriteLine($"Ping: {md.Ping(1000000)}");

        Trace.WriteLine("");

        tf = MarketInfo.TimeFrame.M1;
        var bars = md.GetBars(symbols[0], tf, 1280);

        Trace.WriteLine($"Bars {tf}: {bars.Length}    first: {bars.First()}");

        tf = MarketInfo.TimeFrame.M15;
        bars = md.GetBars(symbols[0], tf, 1280);

        Trace.WriteLine($"Bars {tf}: {bars.Length}    first: {bars.First()}");

        tf = MarketInfo.TimeFrame.H4;
        bars = md.GetBars(symbols[0], tf, 1280);

        Trace.WriteLine($"Bars {tf}: {bars.Length}    first: {bars.First()}");

        tf = MarketInfo.TimeFrame.MN1;
        bars = md.GetBars(symbols[0], tf, 1280);

        Trace.WriteLine($"Bars {tf}: {bars.Length}    first: {bars.First()}");


        Trace.WriteLine("");

        
        for(int i = 0; i < 20; i++)
        {
            tf = MarketInfo.TimeFrame.M1;
            Trace.WriteLine($"Last bar m1: {md.GetLastBar(symbols[0], tf)}");
            tf = MarketInfo.TimeFrame.M5;
            Trace.WriteLine($"Last bar m5: {md.GetLastBar(symbols[0], tf)}");
            tf = MarketInfo.TimeFrame.M15;
            Trace.WriteLine($"Last bar m15: {md.GetLastBar(symbols[0], tf)}");
            Thread.Sleep(1000);
        }

        
        Trace.WriteLine("\nTest is done");


        md.Shutdown();
    }
}