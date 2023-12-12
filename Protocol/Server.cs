using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Protocol;

public class Server
{
    private Socket serverSock;

    private readonly Dictionary<IPEndPoint, Client> connections = [];

    public Server(string ip, int port, int backlog)
    {
        if(port is > ushort.MaxValue or < 0)
        {
            throw new NetworkInformationException();
        }
        Ip = IPAddress.Parse(ip);
        Port = port;
        BackLog = backlog;
    }


    public delegate void ServerStatus(Server server);
    public delegate void ClientStatus(Server server, Client client);


    /// <summary>
    /// Происходит когда подключился новый пользователь
    /// </summary>
    public event ClientStatus UserConnected;


    /// <summary>
    /// Происходит когда сервер стартовал
    /// </summary>
    public event ServerStatus ServerStarted;
    /// <summary>
    /// Происходит когда сервер остановился
    /// </summary>
    public event ServerStatus ServerStopped;





    /// <summary>
    /// Запущен ли сервер
    /// </summary>
    public bool IsStarted { get; private set; }
    /// <summary>
    /// Очередь ожидания для новых подключений
    /// </summary>
    public int BackLog { get; }
    /// <summary>
    /// IP адресс сервера
    /// </summary>
    public IPAddress Ip { get; }
    /// <summary>
    /// Порт сервера
    /// </summary>
    public int Port { get; }



    /// <summary>
    /// Запуск сервера и прослушки подключений
    /// </summary>
    public void Start(bool useAnotherThread)
    {
        if(IsStarted) return;
        serverSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        serverSock.Bind(new IPEndPoint(Ip, Port));
        serverSock.Listen(BackLog);
        IsStarted = true;

        ServerStarted?.Invoke(this);
        if(useAnotherThread)
            Task.Run(Listening);
        else
            Listening();
    }
    /// <summary>
    /// Остановка сервера и прослушки подключений
    /// </summary>
    public void Stop()
    {
        if(!IsStarted) return;
        serverSock?.Close();
        IsStarted = false;

        foreach(var connection in connections)
        {
            connection.Value.Close();
        }

        ServerStopped?.Invoke(this);
    }

    /// <summary>
    /// Прослушка подключений
    /// </summary>
    private void Listening()
    {
        while(IsStarted)
        {
            try
            {
                var c = new Client(serverSock?.Accept());
                connections.Add(c.EndPoint, c);
                UserConnected?.Invoke(this, c);
                c.ClientDisconnected += CcOnClientDisconnected;
                c.StartListen();
            }
            catch(SocketException ex) when(ex.ErrorCode == 10004)
            {
                break;
            }
            catch(ObjectDisposedException)
            {
                break;
            }
        }
    }

    private void CcOnClientDisconnected(Client client)
    {
        client.ClientDisconnected -= CcOnClientDisconnected;
        connections.Remove(client.EndPoint);
    }
}