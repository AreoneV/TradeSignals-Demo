using System.Diagnostics;
using Protocol;

namespace Controller;

public class ServiceObject
{
    
    public string Name { get; set; }
    public string FullPath { get; set; }
    public int Id { get; set; }
    public string Ip { get; set; }
    public int Port { get; set; }

    public Process Process { get; set; }
    public Client Client { get; set; }




    
}