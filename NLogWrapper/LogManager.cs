using System;
using NLog;

namespace NLogWrapper
{
    public static class LogManager
    {
        public static NLogWrapper.ILogger CreateLogger(Type T, string logLevel, string fallbackPath=null)
        {
            return new Logger(T, logLevel, fallbackPath);
        }
        public static NLogWrapper.ILogger CreateLogger(Type T, ILogLevel logLevel, string fallbackPath = null)
        {
            //leave for compaitibility
            return new Logger(T, logLevel.ToString(), fallbackPath);
        }
        
        // Most easy one
        public static NLogWrapper.ILogger CreateLogger(Type T)
        {
            return new Logger(T, "Debug");
        }


    }
}
