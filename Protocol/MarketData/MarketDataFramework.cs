using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.MarketData;
public class MarketDataFramework(string ip, int port)
{
    private readonly Client client = new(ip, port);




    public (DateTime first, DateTime last) GetExtremeDate(TimeFrame timeFrame)
    {
        if(!client.IsConnected)
        {
            client.Connect();
        }

        using MemoryStream memoryStream = new();
        BinaryWriter writer = new(memoryStream);
        writer.Write((int)CommonCommand.SpecialCommand);
        writer.Write((int)MarketDataCommand.GetExtremeDate);
        writer.Write((int)timeFrame);

        var answer = client.Request(memoryStream.ToArray());
        writer.Close();
        memoryStream.Close();

        using MemoryStream answerStream = new(answer);
        var reader = new BinaryReader(answerStream);

        var first = new DateTime(reader.ReadInt64());
        var last = new DateTime(reader.ReadInt64());

        reader.Close();
        answerStream.Close();

        return (first, last);
    }



    public Bar GetLastBar(TimeFrame timeFrame)
    {
        if(!client.IsConnected)
        {
            client.Connect();
        }

        using MemoryStream memoryStream = new();
        BinaryWriter writer = new(memoryStream);
        writer.Write((int)CommonCommand.SpecialCommand);
        writer.Write((int)MarketDataCommand.GetLasBar);
        writer.Write((int)timeFrame);

        var answer = client.Request(memoryStream.ToArray());
        writer.Close();
        memoryStream.Close();
    
        using MemoryStream answerStream = new(answer);
        var reader = new BinaryReader(answerStream);

        var b = Bar.Create(reader);

        reader.Close();
        answerStream.Close();

        return b;

    }
}
