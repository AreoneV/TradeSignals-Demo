using MarketInfo;
using Protocol;
using Protocol.MarketData;

namespace MarketData;

/// <summary>
/// Главный объект сервиса, управляет всем функционалом
/// </summary>
/// <param name="myIp">Ip для запуска сервиса</param>
/// <param name="myPort">Порт для запуска сервиса</param>
internal class Service(string myIp, int myPort)
{
    //Путь к файлу логов
    private const string LogFileName = "logs.txt";
    //логгер
    private readonly StreamWriter writer = new(LogFileName, true);
    //история символов
    private readonly Dictionary<string, Symbol> symbols = [];
    //будущие бары для генерации
    private readonly Dictionary<string, (List<Bar> list, int index)> future = [];
    //максимальная длинна лога для красивой отрисовки
    private int maxLineLength = 1;
    //объект сервера для слушки подключений
    private readonly Server server = new(myIp, myPort, 10);
    //для проверки отдельного потока, работает ли сервис или нет
    private bool isRunning = true;
    //Пауза между генерацией, каждое N миллисекунд генерирует 1 минуту рыночных данных
    private const int sleepImitation = 500; 



    /// <summary>
    /// Запуск сервиса
    /// </summary>
    /// <returns>Возвращает код работы программы</returns>
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
            //запуск сервера
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

    
    /// <summary>
    /// Останавливает и закрывает сервис
    /// </summary>
    /// <param name="reason">Причина остановки</param>
    public void Close(string reason)
    {
        LogWarning($"Server is stopping... Reason: {reason}");
        server.Stop();
    }

    /// <summary>
    /// Загрузка одного символа, исторические данные и ьудущие для генерации
    /// </summary>
    /// <param name="name">Имя символа</param>
    /// <param name="digits">Количество цифр после запятой</param>
    /// <param name="historyFile">Файл с историей баров</param>
    /// <param name="futureFile">Файл с будущими барами</param>
    private void Load(string name, int digits, string historyFile, string futureFile)
    {
        //открываем файл с историей
        var fs = File.OpenRead(historyFile);
        var br = new BinaryReader(fs);
        //создаем объект символа
        Symbol symbol = new(name, digits);
        //создаем историю с барами в 1 минуту
        var hst = symbol.Histories[TimeFrame.M1];
        //считываем, создаем из бинарго представляения и добавляем
        while (fs.Position < fs.Length)
        {
            hst.Add(Bar.Create(br));
        }
        //создаем историю всех остальных временных рамок и добавляем в них уже считанные минутные бара, они сами автоматически конвертируются в нужную временную рамку
        foreach (var timeFrame in Enum.GetValues<TimeFrame>().OrderBy(x => x))
        {
            if(timeFrame == TimeFrame.M1) continue;

            //для ускорения добавления добавляем в следущую историю из предыдущей а не из М1
            symbol.Histories[timeFrame].Add(hst.Bars);
            hst = symbol.Histories[timeFrame];
        }
        //добавляем символ
        symbols.Add(name, symbol);

        br.Close();
        fs.Dispose();
        //открываем файл будущих баров
        fs = File.OpenRead(futureFile);
        br = new BinaryReader(fs);

        var list = new List<Bar>((int)fs.Length / Bar.BarLength);
        //считываем по тому же принципу
        while(fs.Position < fs.Length)
        {
            list.Add(Bar.Create(br));
        }

        future.Add(name, (list, 0));

        br.Close();
        fs.Dispose();
    }
    /// <summary>
    /// В отдельном потоке именируем работу ранка
    /// </summary>
    private void ImitationWorking()
    {
        isRunning = true;
        Task.Run(() =>
        {
            //пока работаем
            while(isRunning)
            {
                try
                {
                    //бегаем по всем символам
                    foreach(var item in future)
                    {
                        //проверяем есть ли еще данные для имитации, если нет выходим из потока
                        if(item.Value.index >= item.Value.list.Count) return;
                        //берем имя символа
                        var symbol = item.Key;
                        //берем список и индекс текущей генерации
                        var (list, index) = future[symbol];
                        //получаем бар и увеличиваем индекс
                        var bar = list[index];
                        index++;
                        //присваеваем изменения
                        future[symbol] = (list, index);
                        //берем символ
                        var sym = symbols[symbol];
                        //бегаем по каждой истории и закидываем в историю бар из будущей истории, таким образом создается имитация работы рынка, простая но для наших задачь более чем достаточно
                        foreach (var tf in Enum.GetValues<TimeFrame>())
                        {
                            var h = sym.Histories[tf];
                            h.Add(bar);
                        }
                    }
                    //пауза на указанное значение
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


    //логирование
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
        var m = $"{DateTime.Now:G} | Warn\t| {msg}";
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
    //разделитель логов
    private void LogSplit()
    {
        writer.WriteLine(new string('_', maxLineLength));
        writer.WriteLine();
        writer.Flush();
    }

    /// <summary>
    /// Когда сервер запустился
    /// </summary>
    /// <param name="srv"></param>
    private void ServerOnServerStarted(Server srv)
    {
        LogInfo("Server is running...");
        //запуск имитации
        ImitationWorking();
    }
    /// <summary>
    /// Когда есть новое соединение
    /// </summary>
    /// <param name="srv"></param>
    /// <param name="client"></param>
    private void ServerOnUserConnected(Server srv, Client client)
    {
        client.ReceivedRequest += ClientOnReceivedRequest;
        client.ClientDisconnected += ClientOnClientDisconnected;
    }
    /// <summary>
    /// Когда соединение разорвано
    /// </summary>
    /// <param name="client"></param>
    private void ClientOnClientDisconnected(Client client)
    {
        client.ReceivedRequest -= ClientOnReceivedRequest;
        client.ClientDisconnected -= ClientOnClientDisconnected;
    }
    /// <summary>
    /// Когда получен новый запрос
    /// </summary>
    /// <param name="client"></param>
    private void ClientOnReceivedRequest(Client client, byte[] message)
    {
        //данные для считывания
        using var ms = new MemoryStream(message);
        var reader = new BinaryReader(ms);
        //данные для ответа
        using var answer = new MemoryStream();
        var writer = new BinaryWriter(answer);
        try
        {
            //получаем команду
            var common = (CommonCommand)reader.ReadInt32();
            switch(common)
            {
                //если команда выключить
                case CommonCommand.Shutdown:
                    //закрываем
                    Close("Received shutdown command!");
                    //отправляем пустой ответ
                    client.SendAnswer([]);
                    break;
                //специальная команда
                case CommonCommand.SpecialCommand:
                    //берем ее и отправляем на обработку
                    var specCommand = (MarketDataCommand)reader.ReadInt32();
                    ProcessingCommand(specCommand, reader, writer);
                    //отправляем ответ
                    client.SendAnswer(answer.ToArray());
                    break;
                case CommonCommand.Logs:
                    //проверяем есть ли файл
                    if(!File.Exists(LogFileName))
                    {
                        writer.Write("There isn't logs");
                        client.SendAnswer(answer.ToArray());
                        return;
                    }
                    //считываем и отправляем
                    writer.Write(File.ReadAllText(LogFileName));
                    client.SendAnswer(answer.ToArray());
                    return;
                default:
                    //в остальных случаях просто отправляем то что нам прислали
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
    /// <summary>
    /// Обработка специальных запросов
    /// </summary>
    /// <param name="command">Команда</param>
    /// <param name="reader">Считывание данных которые пришли вместе с командой</param>
    /// <param name="writer">Запись данных для отправки ответа</param>
    private void ProcessingCommand(MarketDataCommand command, BinaryReader reader, BinaryWriter writer)
    {
        string symbol;
        TimeFrame tf;
        switch(command)
        {
            //запрос на получение символов
            case MarketDataCommand.GetSymbolNames:
                //отправляем длинну и каждый символ
                writer.Write(symbols.Count);
                foreach(var symbolName in symbols.Keys)
                {
                    writer.Write(symbolName);
                }
                break;
            case MarketDataCommand.GetLasBar:
                //считываем символ и временную рамку
                symbol = reader.ReadString();
                tf = (TimeFrame)reader.ReadInt32();
                try
                {
                    //записываем последний бар
                    symbols[symbol].Histories[tf].LastBar.Save(writer);
                }
                catch
                {
                    (new Bar()).Save(writer);
                }
                break;
            case MarketDataCommand.GetExtremeDate:
                //считываем символ и временную рамку
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
                //считываем символ, временную рамку и количество баров
                symbol = reader.ReadString();
                tf = (TimeFrame)reader.ReadInt32();
                int count = reader.ReadInt32();
                try
                {
                    //берем историю и выдираем с конца нужное количество баров
                    var h = symbols[symbol].Histories[tf];
                    var list = h.Bars.TakeLast(count);
                    //пишем количество и пишем сами бары
                    writer.Write(list.Count());
                    foreach (var bar in list)
                    {
                        bar.Save(writer);
                    }
                }
                catch
                {
                    writer.Write(1);
                    (new Bar()).Save(writer);
                }
                break;
            default:
                writer.Write("Error command");
                break;
        }
    }
}