using Services;
using System.Net;

namespace AI;

public static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    //���� ���������� ��������� ���� � enum ExitCode
    static int Main(string[] args)
    {
        //��������� ��� �� � ������� � ���������� ������� � ������
        if(args.Length != 2 || !IPAddress.TryParse(args[0], out _) || !int.TryParse(args[1], out int port) || port > ushort.MaxValue)
        {
            return (int)ExitCode.InvalidArgs;
        }
        //������� � ��������� ��������� �������
        Service service = new(args[0], port);
        return (int)service.Run();
    }
}