using System.Collections.ObjectModel;
using Services;

namespace Controller;
/// <summary>
/// Управляет всеми сервисами
/// </summary>
public class ServiceManagement
{
    //файл настроек сервисов, их пути адреса и авто запуски
    private const string SettingFile = "services.txt";
    //файл логов
    private const string LogFile = "logs.txt";

    //словарь сервисов
    private readonly Dictionary<ServiceNames, ServiceObject> services;
    //писатель логов
    private static readonly StreamWriter Writer = new(LogFile, true);
    //максимальная длинна лога для красивой отрисовки черты
    private static int _maxLineLength = 1;


    private ServiceManagement()
    {
        services = new Dictionary<ServiceNames, ServiceObject>();
        Services = services.AsReadOnly();
    }

    
    /// <summary>
    /// Словарь сервисов для чтения
    /// </summary>
    public ReadOnlyDictionary<ServiceNames, ServiceObject> Services { get; }
    /// <summary>
    /// Запущено ли управление
    /// </summary>
    public bool IsStarted { get; private set; }



    /// <summary>
    /// Запуск всех сервисов и прослушки
    /// </summary>
    public void Start()
    {
        //проверяем не запущено ли управление
        if(IsStarted) return;

        //максимальная длинна точек для стиля
        const int maxLen = 30;

        LogInfo("Common starting...");

        Console.Write("Status: ");

        Console.ForegroundColor = ConsoleColor.Yellow;

        Console.WriteLine("Wait...");
        Console.ResetColor();

        var indent = new string(' ', 4);

        foreach(var service in services)
        {
            //запускаем сервисы по отдельности
            Console.Write($"{indent}{service.Value.Name}{new string('.', maxLen - service.Value.Name.ToString().Length)}");
            service.Value.Start();
            var stat = service.Value.IsRunning ? "Running" : "Stopped";
            Console.ForegroundColor = service.Value.IsRunning ? ConsoleColor.Green : ConsoleColor.Yellow;
            Console.WriteLine($"{stat}");
            Console.ResetColor();
        }
        //выводим информацию о запуске
        IsStarted = true;
        LogInfo("Controller is running");
        CheckRunning();

        Console.Clear();
        WriteInfo();
    }
    /// <summary>
    /// Остановка всех сервисов и прослушки
    /// </summary>
    public void Stop()
    {
        //проверяем не остановлен ли
        if(!IsStarted)
        {
            return;
        }
        LogInfo("Common stopping...");
        IsStarted = false;
        //останавливаем каждый сервис
        foreach(var service in services)
        {
            service.Value.Stop();
        }
        //выводим информацию
        LogInfo("Controller has been stopped");
        Console.Clear();
        WriteInfo();
    }
    /// <summary>
    /// Отправить всем сервисам список адресов и портов всех сервисов
    /// </summary>
    public void SendUpdate()
    {
        //проверка запуска
        if(!IsStarted) { return; }
        //создаем бинарное представление
        using var ms = new MemoryStream();
        var w = new BinaryWriter(ms);
        foreach((ServiceNames key, ServiceObject value) in services)
        {
            //пишем сведения о сервисе
            //его имя
            w.Write((int)key);
            //IP
            w.Write(value.Ip);
            //Port
            w.Write(value.Port);
            //запущен ли
            w.Write(value.IsRunning);
        }
        //превращаем в массив байтов и закрываем поток
        var data = ms.ToArray();
        w.Close();

        foreach((ServiceNames _, ServiceObject value) in services)
        {
            //бегаем по всем сервисам и проверяем запущены ли они для оправки
            if(!value.IsRunning) continue;
            try
            {
                lock(value.Client)
                {
                    //отправляем
                    value.Client.Request(data, 1000);
                }
            }
            catch
            {
                // ignored
            }

        }
    }
    /// <summary>
    /// Очистить консоль
    /// </summary>
    public void Clear()
    {
        Console.Clear();
        WriteInfo();
    }
    /// <summary>
    /// Проверка подключения сервисов
    /// </summary>
    private void CheckRunning()
    {
        Task.Run(() =>
        {
            while(IsStarted)
            {
                //берем все запущенные процессы
                foreach(var o in services.Where(o => o.Value.IsRunning))
                {
                    try
                    {
                        lock(o.Value.Client)
                        {
                            //отправляем тестовый пакет
                            o.Value.Client.Request(new byte[100], 1000);
                            //если ответил то все хорошо
                        }
                    }
                    catch
                    {
                        //если не ответил то пишем лог
                        LogWarning($"{o.Value.Name} is not responding!");
                    }
                }
                Thread.Sleep(1000);
            }

        });
    }
    /// <summary>
    /// Вывести информацию о сервисах
    /// </summary>
    public void WriteInfo()
    {
        const int maxLen = 30;

        Console.Write("Status: ");

        Console.ForegroundColor = IsStarted ? ConsoleColor.Green : ConsoleColor.Red;
        //выводим общее состояние
        var status = IsStarted ? "Running" : "Stopped";
        Console.WriteLine(status);
        Console.ResetColor();

        var indent = new string(' ', 4);

        foreach(var service in services)
        {
            //бегаем по каждому сервису и выводим его состояние
            Console.Write($"{indent}{service.Value.Name}{new string('.', maxLen - service.Value.Name.ToString().Length)}");
            var stat = service.Value.IsRunning ? "Running" : "Stopped";
            Console.ForegroundColor = service.Value.IsRunning ? ConsoleColor.Green : ConsoleColor.Yellow;
            Console.WriteLine($"{stat}");
            Console.ResetColor();
        }
    }
    /// <summary>
    /// Загрузка файла настроек и создание экземпляров
    /// </summary>
    public void Load()
    {
        //Создаем экземпляры всех сервисов со стандартными настройками
        foreach(var value in Enum.GetValues<ServiceNames>())
        {
            services.Add(value, new ServiceObject(value, "127.0.0.1", Directory.GetCurrentDirectory() + $"\\{value}.exe"));
            services[value].EventRunningStatusChanged += OnEventRunningStatusChanged;
        }
        //если нет файла настроек
        if(!File.Exists(SettingFile))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Ok");
            Console.ResetColor();
            return;
        }

        using StreamReader r = new StreamReader(SettingFile);

        while(!r.EndOfStream)
        {
            //читаем строку
            var line = r.ReadLine()?.Trim();
            if(line == null) continue;
            //если начинается с решетки то это комментарий и пропускаем его
            if(line.StartsWith("#")) continue;

            //делим строку на поля
            var args = line.Split(' ');
            //если полей не верное количество
            if(args.Length != 4) continue;
            //проверяем правильное ли имя сервиса
            if(!Enum.TryParse(typeof(ServiceNames), args[0], out object result)) continue;

            var name = (ServiceNames)result;
            var s = Services[name];
            //устанавливаем ip  
            s.Ip = args[2];
            //устанавливаем порт
            s.FullPath = args[1];
            //устанавливаем авто запуск
            s.AutoStart = !bool.TryParse(args[3], out bool auto) || auto;
        }
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Ok");
        Console.ResetColor();
    }
    /// <summary>
    /// Если изменился статус какого-то сервиса
    /// </summary>
    /// <param name="service"></param>
    /// <param name="status"></param>
    private void OnEventRunningStatusChanged(ServiceObject service, bool status)
    {
        //проверка на работу
        if(!IsStarted) return;
        //выводим инфо и отправляем обновленную инфу всем
        Console.Clear();
        WriteInfo();
        LogWarning($"{service.Name} changed status: {status}");
        SendUpdate();
    }

    /// <summary>
    /// Пишет информационный лог
    /// </summary>
    /// <param name="msg"></param>
    public static void LogInfo(string msg)
    {
        var m = $"{DateTime.Now:G} | Info\t| {msg}";
        Writer.WriteLine(m);
        Writer.Flush();
        if(m.Length > _maxLineLength)
        {
            _maxLineLength = m.Length;
        }
    }
    /// <summary>
    /// Пишет лог для обращения внимания
    /// </summary>
    /// <param name="msg"></param>
    public static void LogWarning(string msg)
    {
        var m = $"{DateTime.Now:G} | Warn\t| {msg}";
        Writer.WriteLine(m);
        Writer.Flush();
        if(m.Length > _maxLineLength)
        {
            _maxLineLength = m.Length;
        }
    }
    /// <summary>
    /// Пишет лог ошибки
    /// </summary>
    /// <param name="msg"></param>
    public static void LogError(string msg)
    {
        var m = $"{DateTime.Now:G} | Error\t| {msg}";
        Writer.WriteLine(m);
        Writer.Flush();
        if(m.Length > _maxLineLength)
        {
            _maxLineLength = m.Length;
        }
    }
    /// <summary>
    /// Пишет разделитель логов
    /// </summary>
    public static void LogSplit()
    {
        Writer.WriteLine(new string('_', _maxLineLength));
        Writer.WriteLine();
        Writer.Flush();
    }
    /// <summary>
    /// Закрывает логирование
    /// </summary>
    public static void LogClose()
    {
        Writer.Flush();
        Writer.Close();
    }

}