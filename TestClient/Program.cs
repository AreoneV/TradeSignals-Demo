using Protocol;

namespace TestClient;

internal class Program
{
    static void Main(string[] args)
    {
        Client client = new Client("127.0.0.1", 41222);



        Console.ReadLine();

        client.Connect();
        Console.WriteLine("I'm connected!");
        Console.ReadLine();

        client.Close();

        if (!client.IsConnected)
        {
            Console.WriteLine("I disconnected.");
        }

        Console.ReadLine();


    }

}
