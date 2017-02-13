using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLogWrapper
{
    public static class LogManager
    {
        public static ILogger CreateLogger(Type T)
        {
            return new Logger(T);
        }
    }
}
