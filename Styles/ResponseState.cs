using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoScanFQCTest
{
    public enum ResponseState
    {
        Exception = -3,
        TimeOut = -2,
        Error = -1,
        Warn = 0,
        Normal = 1,
    }
}
