using System.Diagnostics;
using System.Net.NetworkInformation;
using Protocol;
using Services;

namespace Controller;

/// <summary>
/// Объект одного сервиса, хранит информацию и управляет сервисом
/// </summary>
/// <param name="name">Статическое имя</param>
/// <param name="ip">IP адрес</param>
/// <param name="fullPath">Полный путь к исполняемому файлу</param>
public class ServiceObject(ServiceNames name, string ip, string fullPath)
{
    //диапазон портов для проверки свободных
    private readonly Range portRange = new(41222, 41300);

    /// <summary>
    /// Статическое имя сервиса
    /// </summary>
    public ServiceNames Name { get; } = name;
    /// <summary>
    /// Полный путь к исполняемому файлу
    /// </summary>
    public string FullPath { get; set; } = fullPath;
    /// <summary>
    /// IP адрес запуска
    /// </summary>
    public string Ip { get; set; } = ip;
    /// <summary>
    /// Запускается ли сервис автоматически при сбое в работе
    /// </summary>
    public bool AutoStart { get; set; } = true;
    /// <summary>
    /// Выделенный порт для сервиса
    /// </summary>
    public int Port { get; private set; }

    /// <summary>
    /// Процесс исполняемого файла
    /// </summary>
    public Process Process { get; set; }
    /// <summary>
    /// Подключение к сервису
    /// </summary>
    public Client Client { get; set; }

    /// <summary>
    /// True если сервис запущен и работает. В противном случае false
    /// </summary>
    public bool IsRunning { get; private set; }


    public delegate void RunningDelegate(ServiceObject service, bool status);
    /// <summary>
    /// происходит когда значение свойства IsRunning меняется
    /// </summary>
    public event RunningDelegate EventRunningStatusChanged;

    /// <summary>
    /// Запуск сервиса
    /// </summary>
    public void Start()
    {
        //проверяем не запущен ли сервис
        if(IsRunning) { return; }
        ServiceManagement.LogInfo($"{Name} is starting");
        //проверяем есть ли исполняемый файл
        if(!File.Exists(FullPath))
        {
            ServiceManagement.LogError($"{Name} was not started. Executable file not found.");
            return;
        }

        //проверяем запущены ли копии этого сервиса, если да то уничтожаем их 
        var localByName = Process.GetProcessesByName($"{Name}");
        if (localByName.Length > 0)
        {
            ServiceManagement.LogWarning($"Found {localByName.Length} the same processes. They will be killed.");
        }
        foreach(Process p in localByName)
        {
            p.Kill();
        }

        //присвоим свободный порт
        Port = GetFreePort();
        //запускаем процесс
        Process = Process.Start(FullPath, $"{Ip} {Port}")!;
        ServiceManagement.LogInfo("Process has started");
        //объявляем соединение с ip и выделенным портом
        Client = new Client(Ip, Port);

        //количество попыток
        int attempts = 5;

        do
        {
            try
            {
                //пробуем подключиться к запущенному процессу
                Client.Connect();
                Client.ClientDisconnected += ClientOnClientDisconnected;
                ServiceManagement.LogInfo("Connection is established");
                break;
            }
            catch
            {
                //если не получилось
                //проверяем не был ли закрыт процесс
                if (Process.HasExited)
                {
                    ServiceManagement.LogError("Starting failed. Process has exited.");
                    Stop();
                    return;
                }
                //если нет но отнимаем попытку
                attempts--;
                ServiceManagement.LogWarning($"Connection failed. There are still attempts left: {attempts}");
            }
            //ждем секунду и пробуем снова
            Thread.Sleep(1000);
        } while (attempts > 0);

        //если после успешного подключения разорвалось соединение значит что то пошло не так в работе сервиса
        if (!Client.IsConnected)
        {
            ServiceManagement.LogError("Starting failed. Connection has closed.");
            Stop();

            return;
        }
        //уведомляем о запуске
        ServiceManagement.LogInfo($"{Name} has started");
        IsRunning = true;
        EventRunningStatusChanged?.Invoke(this, true);
    }
    /// <summary>
    /// Остановка сервиса
    /// </summary>
    public void Stop()
    {
        //проверка не остановлен ли
        if(!IsRunning) { return; }
        ServiceManagement.LogInfo($"{Name} is stopping");
        //проверяем есть ли подключение и не null ли клиент
        if (Client is { IsConnected: true })
        {
            try
            {
                //отправляем сообщение о том что бы сервис сам себя завершил
                Client.Request(new []{ (byte)128 }, 1000);
            }
            finally
            {
                //разрываем соединение
                Client.ClientDisconnected -= ClientOnClientDisconnected;
                Client.Close();
                ServiceManagement.LogInfo("Connection has closed");
            }
        }

        //проверяем работает ли процесс и не null ли он
        // ReSharper disable once InvertIf
        if(Process is { HasExited: false})
        {
            try
            {
                //если он работает то ждем 10 секунд на самостоятельное закрытие
                if(!Process.WaitForExit(10000))
                {
                    //если не дождались просто убиваем процесс
                    Process.Kill();
                    ServiceManagement.LogWarning("Process was killed");
                }
            }
            catch
            {
                //ignored
            }
        }
        //уведомляем и обнуляем
        ServiceManagement.LogWarning($"Process code: {Process.ExitCode}");
        Client = null;
        Process = null;
        IsRunning = false;
        EventRunningStatusChanged?.Invoke(this, false);
    }
    /// <summary>
    /// Проверить пинг соединения
    /// </summary>
    public void Ping()
    {
        //проверяем есть ли соединение
        if(Client is not { IsConnected: true })
        {
            Console.WriteLine("No connection!");
            return;
        }

        
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                //начинаем замер времени
                var sw = Stopwatch.StartNew();
                try
                {
                    lock(Client)
                    {
                        //отправляем пустой пакет в размере от 100 до 1000 байт с шагом в 100 и ждем ответа 100 миллисекунд
                        Client.Request(new byte[(i + 1) * 100], 100); 
                    }
                    //останавливаем замер и выводим результат
                    sw.Stop();
                    Console.WriteLine($"Ping {(i + 1) * 100} bytes --> {sw.Elapsed.TotalMilliseconds:F3} ms");
                }
                catch(TimeoutException)
                {
                    //если сработал timeout в 100 миллисекунд
                    Console.WriteLine($"Ping {(i + 1) * 100} bytes --> >= 100 ms");
                }
                catch
                {
                    //если что-то пошло не так
                    Console.WriteLine("Error connection!");
                    return;
                }
            }
            
        }
    }

    /// <summary>
    /// Получить свободный порт из диапазона
    /// </summary>
    /// <returns>Выделенный свободный порт</returns>
    private int GetFreePort()
    {
        //получаем все активные соединения
        var tcpConnInfoArray = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections();
        //бегаем по диапазону
        for(var i = portRange.Start.Value; i < portRange.End.Value; i++)
        {
            //если в текущих подключениях имеется данный порт то пропускаем его 
            if (tcpConnInfoArray.Any(con => con.LocalEndPoint.Port == i)) continue;

            //если нет то возвращаем
            ServiceManagement.LogInfo($"Got free port: {i}");
            return i;
        }
        //если все порты заняты
        ServiceManagement.LogError("All port is busy");
        return -1;
    }
    /// <summary>
    /// Если соединение разорвано
    /// </summary>
    /// <param name="client"></param>
    private void ClientOnClientDisconnected(Client client)
    {
        //если соединение разорвано
        //проверяем есть ли у нас авто запуск
        if(AutoStart)
        {
            ServiceManagement.LogWarning($"{Name} try to restart");
            //пере запускаем
            Stop();
            Start();
            //проверяем получилось ли
            if (IsRunning)
            {
                ServiceManagement.LogWarning($"Restarting {Name} completed successfully");
                ServiceManagement.LogSplit();
                return;
            }
            //если нет останавливаем
            ServiceManagement.LogError($"Restarting {Name} failed");
            Stop();
            ServiceManagement.LogSplit();
            return;
        }
        //если нет перезапуска тогда все останавливаем
        ServiceManagement.LogError("Something went wrong! Service disconnected");
        Stop();
        ServiceManagement.LogSplit();
    }
}