using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketData;
public enum ExitCode
{
    Ok = 0,
    Killed,
    InvalidArgs,
    ErrorStarting
}
