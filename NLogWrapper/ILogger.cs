using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLogWrapper
{
    public interface ILogger
    {
        void Info(string msg, params object[] args);
        void Warn(string msg, params object[] args);
        void Error(string msg, params object[] args);
        void Debug(string msg, params object[] args);
        void LogFallback(string msg, params object[] args);
    }
}
