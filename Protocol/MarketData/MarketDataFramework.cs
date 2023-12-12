using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.MarketData;
public class MarketDataFramework(string ip, int port)
{
    private readonly Client client = new(ip, port);




    
}
