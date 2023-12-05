using System.Diagnostics;
using Protocol;

namespace Controller;

public class ServiceObject(ServiceNames name, string ip, string fullPath)
{
    public ServiceNames Name { get; } = name;
    public string FullPath { get; set; } = fullPath;
    public string Ip { get; set; } = ip;
    public bool AutoStart { get; set; } = true;
    public int Port { get; set; }

    public Process Process { get; set; }
    public Client Client { get; set; }




    
}