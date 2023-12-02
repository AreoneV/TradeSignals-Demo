using MarketInfo;
// ReSharper disable StringLiteralTypo
﻿namespace MarketData;

namespace MarketData;

public class Service
{
    private const string LogFileName = "logs.txt";

    private readonly StreamWriter writer = new StreamWriter(LogFileName, true);

    private readonly Dictionary<string, Symbol> symbols = new();

    private readonly Dictionary<string, List<Bar>> future = new();

    private int maxLineLength = 1;
    
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
}