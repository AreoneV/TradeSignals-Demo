using System.Net;
using System.Net.Sockets;
// ReSharper disable InconsistentlySynchronizedField

namespace Protocol;

public class Client
{
    //сокет
    private Socket client;

    //Ожидание ответа
    private readonly EventWaitHandle waiting = new EventWaitHandle(false, EventResetMode.AutoReset);
    private byte[] waitData;

    //Для того что бы ошибка не вылетала при ручном закрытии сокета
    private bool nonErr;


    private Client()
    {
        //Сразу подписываемся на получение сообщений
        ReceivedMessage += OnReceivedMessage;
    }



    public Client(string ip, int port) : this()
    {
        EndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
    }
    public Client(IPEndPoint endPoint) : this()
    {
        EndPoint = endPoint;
    }
    internal Client(Socket socket) : this()
    {
        this.client = socket;
        EndPoint = socket.RemoteEndPoint as IPEndPoint;
    }


    public delegate void ConnectionDelegate(Client client);
    public delegate void ReceivedMessageDelegate(Client client, byte[] message);

    /// <summary>
    /// происходит когда клиент отключился
    /// </summary>
    public event ConnectionDelegate ClientDisconnected;



    /// <summary>
    /// Происходит когда получено сообщение
    /// </summary>
    internal event ReceivedMessageDelegate ReceivedMessage;



    /// <summary>
    /// Конечная точка подключения
    /// </summary>
    public IPEndPoint EndPoint { get; }
    /// <summary>
    /// Подключен ли клиент
    /// </summary>
    public bool IsConnected {
        get
        {
            if (client == null) return false;
            try
            {
                bool part1 = client.Poll(1000, SelectMode.SelectRead);
                bool part2 = (client.Available == 0);
                return !part1 || !part2;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Плдключение к серверу
    /// </summary>
    public void Connect()
    {
        if(IsConnected) return;
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        client.Connect(EndPoint);
        StartListen();
    }
    /// <summary>
    /// Отключение от  ервера и закрытие сокета
    /// </summary>
    public void Close()
    {
        try
        {
            nonErr = true;
            client?.Close();
        }
        finally
        {
            ClientDisconnected?.Invoke(this);
        }
    }

    /// <summary>
    /// Запрос на сервер и получить ответ
    /// </summary>
    /// <param name="message">Сообщение запроса</param>
    /// <param name="timeOut">Таймаут</param>
    /// <returns>Возвращает ответ от сервера</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="TimeoutException"></exception>
    public byte[] Request(byte[] message, int timeOut = 15000)
    {
        if(message == null) throw new ArgumentNullException(nameof(message));

        waitData = null;
        //отправляем сообщение
        try
        {
            SendMessage(message);
        }
        catch
        {
            client?.Close();
            if(!nonErr)
                ClientDisconnected?.Invoke(this);
            nonErr = false;
            return null;
        }
        //ждем ответ
        waiting.WaitOne(timeOut);
        if (waitData == null) throw new TimeoutException();

        return waitData;
    }
    /// <summary>
    /// Отправка ответа на сообщение
    /// </summary>
    /// <param name="message">Сообщение на которое нужно ответить</param>
    internal void SendAnswer(byte[] message)
    {
        try
        {
            SendMessage(message);
        }
        catch
        {
            client?.Close();
            if(!nonErr)
                ClientDisconnected?.Invoke(this);
            nonErr = false;
        }
    }



    //Слушаем входящие сообщения
    internal void StartListen()
    {
        Task.Run(() =>
        {
            while(IsConnected)
            {
                try
                {
                    while(IsConnected)
                    {
                        ReceivedMessage?.Invoke(this, Receive());
                    }
                }
                catch
                {
                    client?.Close();
                    if(!nonErr)
                        ClientDisconnected?.Invoke(this);
                    nonErr = false;
                }
            }
        });
    }


    //Отправить сообщений
    private void SendMessage(byte[] data)
    {
        lock(client)
        {
            //Отправка длинны пакета
            byte[] buffer = BitConverter.GetBytes(data.Length);
            client.Send(buffer);
            //отправка всего сообщения
            int len = 0;
            while(true)
            {
                len += client.Send(data, len, data.Length - len, SocketFlags.None);
                if(len == data.Length)
                {
                    break;
                }
            }
        }
    }

    //Получить сообщение
    private byte[] Receive()
    {
        const int intSize = sizeof(int);

        //получаем длинну пакета
        byte[] buffer = new byte[intSize];
        client.Receive(buffer, intSize, SocketFlags.None);
        int bufferSize = BitConverter.ToInt32(buffer, 0);

        buffer = new byte[bufferSize];

        //получаем остальные данные
        int len = 0;
        while(true)
        {
            len += client.Receive(buffer, len, bufferSize - len, SocketFlags.None);
            if(len == bufferSize)
            {
                break;
            }
        }

        return buffer;
    }


    //при получении какого либо сообщения проверить его id не ждут ли его как ответ
    private void OnReceivedMessage(Client c, byte[] m)
    {
        waitData = m;
        waiting.Set();
    }

}