using System;
using NLog;

namespace NLogWrapper
{
    public static class LogManager
    {
        public enum ILogLevel { Off, Info, Error, Debug, Trace };
        public static NLogWrapper.ILogger CreateLogger(Type T, string logLevel, string fallbackPath=null)
        {
            return new Logger(T, String2Enum(logLevel), fallbackPath);
        }
        public static NLogWrapper.ILogger CreateLogger(Type T, ILogLevel logLevel, string fallbackPath = null)
        {
            return new Logger(T, logLevel, fallbackPath);
        }
        
        // Most easy one
        public static NLogWrapper.ILogger CreateLogger(Type T)
        {
            return new Logger(T, ILogLevel.Debug);
        }

        private static ILogLevel String2Enum(string value)
        {
            ILogLevel result = ILogLevel.Debug;
            Enum.TryParse(InitCap(value), out result);
            return result;
        }
        private static string InitCap(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }
    }
}
