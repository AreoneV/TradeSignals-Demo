using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services;
public enum ExitCode
{
    Ok = 0,
    Killed,
    InvalidArgs,
    ErrorStarting
}
