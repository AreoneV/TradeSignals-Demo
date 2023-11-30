using System.Text;
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

        var m = "Hi, server. How are you?"u8.ToArray();

        var a = client.Request(m);

        if (a != null)
        {
            Console.WriteLine($"Answer from server: {Encoding.UTF8.GetString(a)}");
        }
        else
        {
            Console.WriteLine("Answer is null!");
        }
        Console.ReadLine();

        client.Close();

        Console.ReadLine();


    }

    private static void ClientOnClientDisconnected(Client client)
    {
        Console.WriteLine("I disconnected.");
    }
}
