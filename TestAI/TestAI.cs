using System.Diagnostics;
using MarketInfo;
using Microsoft.VisualBasic.Logging;
using Protocol.AI;

namespace TestAI;

[TestClass]
public class TestAI
{
    [TestMethod]
    public void TestCommonCommands()
    {
        Task.Run(() =>
        {
            var err = AI.Program.Main(["127.0.0.1", "41222"]);

            if(err != 0) Assert.Fail();

            Trace.WriteLine($"Exit code: {err}");
        });


        AIFramework f = new AIFramework("127.0.0.1", 41222);

        var logs = f.GetLogs();

        Trace.WriteLine($"Logs: {logs.Length}");
        Trace.WriteLine("");
        foreach (var log in logs)
        {
            Trace.WriteLine(log);
        }
        Trace.WriteLine("");
        Trace.WriteLine($"Ping: {f.Ping(1000000)}");
        f.Shutdown();
        Trace.WriteLine("");
        Trace.WriteLine("Done");
    }

    [TestMethod]
    public void TestSpecialCommands()
    {
        Task.Run(() =>
        {
            var err = AI.Program.Main(["127.0.0.1", "41222"]);

            if(err != 0) Assert.Fail();

            Trace.WriteLine($"Exit code: {err}");
        });


        AIFramework f = new AIFramework("127.0.0.1", 41222);

        float[] randomData = new float[200];

        Random rnd = new Random();


        for (var i = 0; i < randomData.Length; i++)
        {
            randomData[i] = rnd.NextSingle();
        }

        Stopwatch sw = Stopwatch.StartNew();
        var p = f.Predict("EURUSD", TimeFrame.H4, randomData);
        sw.Stop();
        Trace.WriteLine($"EUR H4 Prediction buy: {p.buyPrediction}  Prediction sell: {p.sellPrediction} --- Time : {sw.Elapsed.TotalMilliseconds} ms");

        sw.Restart();
        p = f.Predict("EURUSD", TimeFrame.H1, randomData);
        sw.Stop();
        Trace.WriteLine($"EUR H1 Prediction buy: {p.buyPrediction}  Prediction sell: {p.sellPrediction} --- Time : {sw.Elapsed.TotalMilliseconds} ms");

        sw.Restart();
        p = f.Predict("EURUSD", TimeFrame.M30, randomData);
        sw.Stop();
        Trace.WriteLine($"EUR M30 Prediction buy: {p.buyPrediction}  Prediction sell: {p.sellPrediction} --- Time : {sw.Elapsed.TotalMilliseconds} ms");

        sw.Restart();
        p = f.Predict("EURUSD", TimeFrame.M15, randomData);
        sw.Stop();
        Trace.WriteLine($"EUR M15 Prediction buy: {p.buyPrediction}  Prediction sell: {p.sellPrediction} --- Time : {sw.Elapsed.TotalMilliseconds} ms");

        sw.Restart();
        p = f.Predict("EURUSD", TimeFrame.M5, randomData);
        sw.Stop();
        Trace.WriteLine($"EUR M5 Prediction buy: {p.buyPrediction}  Prediction sell: {p.sellPrediction} --- Time : {sw.Elapsed.TotalMilliseconds} ms");



        sw.Restart();
        p = f.Predict("GBPUSD", TimeFrame.H4, randomData);
        sw.Stop();
        Trace.WriteLine($"GBP H4 Prediction buy: {p.buyPrediction}  Prediction sell: {p.sellPrediction} --- Time : {sw.Elapsed.TotalMilliseconds} ms");

        sw.Restart();
        p = f.Predict("GBPUSD", TimeFrame.H1, randomData);
        sw.Stop();
        Trace.WriteLine($"GBP H1 Prediction buy: {p.buyPrediction}  Prediction sell: {p.sellPrediction} --- Time : {sw.Elapsed.TotalMilliseconds} ms");

        sw.Restart();
        p = f.Predict("GBPUSD", TimeFrame.M30, randomData);
        sw.Stop();
        Trace.WriteLine($"GBP M30 Prediction buy: {p.buyPrediction}  Prediction sell: {p.sellPrediction} --- Time : {sw.Elapsed.TotalMilliseconds} ms");

        sw.Restart();
        p = f.Predict("GBPUSD", TimeFrame.M15, randomData);
        sw.Stop();
        Trace.WriteLine($"GBP M15 Prediction buy: {p.buyPrediction}  Prediction sell: {p.sellPrediction} --- Time : {sw.Elapsed.TotalMilliseconds} ms");

        sw.Restart();
        p = f.Predict("GBPUSD", TimeFrame.M5, randomData);
        sw.Stop();
        Trace.WriteLine($"GBP M5 Prediction buy: {p.buyPrediction}  Prediction sell: {p.sellPrediction} --- Time : {sw.Elapsed.TotalMilliseconds} ms");


        f.Shutdown();
        Trace.WriteLine("");
        Trace.WriteLine("Done");
    }
}