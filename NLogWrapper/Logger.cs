using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace NLogWrapper
{
    public class Logger : ILogger
    {
        public  NLog.ILogger _logger;
        private string _fallbackLogPath = Path.Combine(@"C:\temp", "NLogWrapperFallback.log");
        private  static object _basicLockingTarget = new object();
        private string _caller ="NOTSET";
        private bool exceptionLogged = false;
        public Logger(Type T)
        {
            _logger = NLog.LogManager.GetLogger(T.Name);
            _caller = T.Name;
            Configure();
        }

        private void Configure()
        {
            try
            {
                var homeDir = ""; var logdir = ""; var logfile = "";
                if (System.Reflection.Assembly.GetEntryAssembly() != null)
                {
                    homeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().GetName().CodeBase.Replace("file:///", string.Empty));
                }
                else // Web
                {
                    homeDir = AppDomain.CurrentDomain.BaseDirectory;
                }
                var prefix = GetMeaningfulPrefix(homeDir);

                logdir = Path.Combine(homeDir, "logs");
                if (!Directory.Exists(logdir)) Directory.CreateDirectory(logdir);

                logfile = Path.Combine(logdir, string.Format("{0}-${{shortdate}}.log", prefix));

                var config = new LoggingConfiguration();
                var fileTarget = new FileTarget();
                fileTarget.FileName = logfile;
                fileTarget.Layout = "${longdate} ${uppercase:${level}} - ${message}";
                config.AddTarget("file", fileTarget);

                var rule2 = new LoggingRule("*", LogLevel.Debug, fileTarget);
                config.LoggingRules.Add(rule2);
                NLog.LogManager.Configuration = config;
                NLog.LogManager.ThrowExceptions = true;
            }
            catch (Exception ex)
            {
                LogFallback(ex);
                throw;
            }

        }

        private string GetMeaningfulPrefix(string path)
        {
            var binFolder = new DirectoryInfo(path);
            while (binFolder != null && !binFolder.Name.ToLower().Equals("bin"))
                binFolder = binFolder.Parent;

            if (binFolder != null)
            {
                return binFolder.Parent != null ? binFolder.Parent.Name : binFolder.Name;
            }
            else return new DirectoryInfo(path).Name;
        }

        public void LogFallback(string msg, params object[] args)
        {
            lock(_basicLockingTarget)
            {
                try
                {
                    var message = string.Format(msg, ObjArrayToStringArray(args));
                    File.AppendAllLines(_fallbackLogPath, new string[] { message });
                }
                catch (Exception ex)
                {
                    string message = "Error writing fallback message, nr parmaters =" + args != null ? args.Length.ToString() : 0.ToString();
                    File.AppendAllLines(_fallbackLogPath, new string[] { message, msg });
                }
            }
        }

        private string[] ObjArrayToStringArray(object[] args)
        {
            var result = new string[args.Length];
            int i = 0;
            foreach (var o in args)
            {
                result[i] = Convert.ToString(o);
                i++;
            }
            return result.ToArray<string>();
        }

        public void LogFallback(Exception ex)
        {
            if (!exceptionLogged)
            {
                LogFallback(ex.Message);
                exceptionLogged = true;
            }
        }

        public void Info(string msg, params object[] args)
        {
            var msg2 = string.Format("{0} - {1}", _caller, msg); // duh .. NLog ${callsite} does not work due to the wrapping
            try
            {
                _logger.Info(msg2, args);
            }
            catch (Exception ex)
            {
                try
                {
                    //try if its incorrect parameters
                    _logger.Info(msg2);
                    _logger.Error("an Error with the parmater subtitution occurredin previous message");
                }
                catch (Exception ex2)
                {
                    //swallow 2nd try, but log the original error
                    LogFallback(ex);
                    LogFallback(msg2, args);
                }
            }
        }

        public void Warn(string msg, params object[] args)
        {
            var msg2 = string.Format("{0} - {1}", _caller, msg); // duh .. NLog ${callsite} does not work due to the wrapping
            try
            {
                _logger.Warn(msg2, args);

            }
            catch (Exception ex)
            {
                try
                {
                    //try if its incorrect parameters
                    _logger.Warn(msg2);
                    _logger.Error("an Error with the parmater subtitution occurredin previous message");
                }
                catch (Exception ex2)
                {
                    //swallow 2nd try
                }
                LogFallback(ex);
                LogFallback(msg2, args);
            }

        }

        public void Error(string msg, params object[] args)
        {
            var msg2 = string.Format("{0} - {1}", _caller, msg); // duh .. NLog ${callsite} does not work due to the wrapping
            try
            {
                _logger.Error(msg2, args);

            }
            catch (Exception ex)
            {
                try
                {
                    //try if its incorrect parameters
                    _logger.Error(msg2);
                    _logger.Error("an Error with the parmater subtitution occurredin previous message");
                }
                catch (Exception ex2)
                {
                    //swallow 2nd try
                }
                LogFallback(ex);
                LogFallback(msg2, args);
            }

        }
        public void Debug(string msg, params object[] args)
        {
            var msg2 = string.Format("{0} - {1}", _caller, msg); // duh .. NLog ${callsite} does not work due to the wrapping
            try
            {
                _logger.Debug(msg2, args);

            }
            catch (Exception ex)
            {
                try
                {
                    //try if its incorrect parameters
                    _logger.Debug(msg2);
                    _logger.Error("an Error with the parmater subtitution occurredin previous message");
                }
                catch (Exception ex2)
                {
                    //swallow 2nd try
                }
                LogFallback(ex);
                LogFallback(msg2, args);
            }

        }

    }
}
