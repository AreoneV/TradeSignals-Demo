using System.Text;
using Protocol;

namespace TestServer;

internal class Program
{
    static void Main(string[] args)
    {
        Server server = new Server("127.0.0.1", 41222, 10);

        server.ServerStarted += ServerOnServerStarted;
        server.ServerStopped += ServerOnServerStopped;

        server.UserConnected += ServerOnUserConnected;


        server.Start(true);

        Console.ReadLine();

        server.Stop();
        Console.ReadLine();
    }

    private static void ServerOnUserConnected(Server server, ClientConnected client)
    {
        Console.WriteLine("User connected.");
        client.ClientDisconnected += ClientOnClientDisconnected;
        client.ReceivedRequest += ClientOnReceivedRequest;
    }

    private static void ClientOnReceivedRequest(ClientConnected client, byte[] request, out byte[] answer)
    {
        Console.WriteLine($"Received request: {Encoding.UTF8.GetString(request)}");
        answer = "Hi, client. I'm server!"u8.ToArray();
    }

    private static void ClientOnClientDisconnected(ClientConnected client)
    {
        Console.WriteLine("User disconnected.");
    }

    private static void ServerOnServerStopped(Server server)
    {
        Console.WriteLine("Server has stopped!");
    }
    private static void ServerOnServerStarted(Server server)
    {
        Console.WriteLine("Server has started!");
    }
}
