using Protocol.MarketData;

namespace TestMarketData;

[TestClass]
public class TestClient
{
    [TestMethod]
    public void Connection()
    {
        var md = new MarketDataFramework("127.0.0.1", 41222);

        var logs = md.GetLogs();
        Assert.IsNotNull(logs);
    }
}