using System;
using NLog;

namespace NLogWrapper
{
    public static class LogManager
    {
        public enum ILogLevel { Debug, Info, Error, Trace };
        public static NLogWrapper.ILogger CreateLogger(Type T, ILogLevel level, string fallbackPath=null)
        {
            //where to handle Nlog.LogLevel level
            return new Logger(T, level, fallbackPath);
        }
        //old one
        public static NLogWrapper.ILogger CreateLogger(Type T)
        {
            return new Logger(T, ILogLevel.Debug);
        }
    }
}
