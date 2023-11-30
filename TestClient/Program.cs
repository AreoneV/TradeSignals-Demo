using Protocol;

namespace TestClient;

internal class Program
{
    static void Main(string[] args)
    {
        Client client = new Client("127.0.0.1", 41222);

        client.ClientDisconnected += ClientOnClientDisconnected;


        Console.ReadLine();

        client.Connect();
        Console.WriteLine("I'm connected!");
        Console.ReadLine();

        client.Close();

        Console.ReadLine();


    }

    private static void ClientOnClientDisconnected(Client client)
    {
        Console.WriteLine("I disconnected.");
    }
}
