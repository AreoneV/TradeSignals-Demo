using Protocol;

namespace TestService;

internal static class Program
{

    private static Server srv;

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    private static int Main(string[] args)
    {
        
        srv = new Server(args[0], int.Parse(args[1]), 10);
        srv.UserConnected += SrvOnUserConnected;

        srv.Start(false);

        return 0;
    }

    private static void SrvOnUserConnected(Server server, Client client)
    {
        client.ReceivedRequest += Client_ReceivedRequest;
    }

    private static void Client_ReceivedRequest(Client client, byte[] message)
    {
        client.SendAnswer(message);
        if(message[0] == 128)
        {
            srv.Stop();
        }
    }
}