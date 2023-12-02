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