using System.Diagnostics;
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

        for (int i = 0; i < 10; i++)
        {
            var sw = Stopwatch.StartNew();
            var a = client.Request(Encoding.UTF8.GetBytes("Hi server, I want to test speed our connection."));
            sw.Stop();
            Console.WriteLine($"Time: {sw.Elapsed.TotalMilliseconds:F2} ms");
        }

        Console.WriteLine("Done");

        Console.ReadLine();

        client.Close();

        Console.ReadLine();


    }

    private static void ClientOnClientDisconnected(Client client)
    {
        Console.WriteLine("I disconnected.");
    }
}
