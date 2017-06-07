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
using System.Security.Claims;
using System.Threading;
using static NLogWrapper.LogManager;

namespace NLogWrapper
{
    public class Logger : ILogger
    {
        public  NLog.ILogger _logger;
        private string _fallbackLogFileName = string.Format("NLogWrapperFallback-{0}-{1}-{2}.log", DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year);
        private string _fallbackLogPath;
        private  static object _basicLockingTarget = new object();

        private string _callerClass ="NOTSET";
        private bool exceptionLogged = false;
        private ILogLevel _logLevel;

        public Logger(Type T, string level, string fallbackLogfolder = null)
        {
            _fallbackLogPath = Path.Combine(Path.GetTempPath(), _fallbackLogFileName);
            _logLevel = String2Enum(level);
            _logger = NLog.LogManager.GetLogger(T.Name);

            if (_logLevel != ILogLevel.Off)
            {
                if (fallbackLogfolder != null)
                {
                    if (GoodForLogging(fallbackLogfolder))
                        _fallbackLogPath = Path.Combine(fallbackLogfolder, _fallbackLogFileName);
                }
            }
            _callerClass = T.Name;
            NLogConfigure(ILevel2NLogLevel(_logLevel));
        }

        public static ILogLevel String2Enum(string value)
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

        private NLog.LogLevel ILevel2NLogLevel(ILogLevel level)
        {
            // didn't see right away how to do this better ;)
            switch (level)
            {
                case ILogLevel.Trace: return LogLevel.Trace;
                case ILogLevel.Debug: return LogLevel.Debug;
                case ILogLevel.Info: return LogLevel.Info;
                case ILogLevel.Error: return LogLevel.Error;
                case ILogLevel.Off: return LogLevel.Off;
                default: return LogLevel.Debug;
            }
        }
        private bool GoodForLogging(string directory)
        {
            bool success = false;
            string fullPath = Path.Combine(directory, "tempFile.tmp");

            if (Directory.Exists(directory))
            {
                string username = Environment.UserName;
                try
                {
                    using (FileStream fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write))
                    {
                        fs.WriteByte(0xff);
                    }

                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                        success = true;
                    }
                    else
                    {
                        throw new Exception(string.Format("Nlog.zeroconfig: fallback log path test file creation threw no exception but cannot be found! Environment.UserName={0}.", username));
                    }
                }
                catch (Exception)
                {
                    
                    throw new Exception(string.Format("Nlog.zeroconfig: fallback log path should have write access to Environment.UserName= {0}.", username));
                }
            }
            else
            {
                throw new Exception("Nlog.zeroconfig: fallback log path should exist! Omit setting to use Path.GetTempPath().");
            }
            return success;
        }        

        private void NLogConfigure(LogLevel level)
        {
            try
            {
                var homeDir = ""; var logdir = ""; var logpath = "";
                if (System.Reflection.Assembly.GetEntryAssembly() != null)
                {
                    homeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().GetName().CodeBase.Replace("file:///", string.Empty));
                }
                else // Web
                {
                    homeDir = AppDomain.CurrentDomain.BaseDirectory;
                }
                var FilenamePrefix = GetProjectname(homeDir);

                logdir = Path.Combine(homeDir, "logs");
                if (level != LogLevel.Off &&!Directory.Exists(logdir)) Directory.CreateDirectory(logdir);

                logpath = Path.Combine(logdir, string.Format("{0}-${{shortdate}}.log", FilenamePrefix));

                var config = new LoggingConfiguration();
                var fileTarget = new FileTarget();
                fileTarget.FileName = logpath;
                fileTarget.Layout = "${longdate} ${uppercase:${level}} - ${message}";
                config.AddTarget("file", fileTarget);

                var rule = new LoggingRule("*", level, fileTarget);
                config.LoggingRules.Add(rule);
                NLog.LogManager.Configuration = config;

                NLog.LogManager.ThrowExceptions = true;
            }
            catch (Exception ex)
            {
                LogFallback(ex);
                throw;
            }

        }

        private string GetProjectname(string path)
        {
            // Path is either projectName/logs/ or projectName/bin/debug/
            DirectoryInfo binFolder = FindBinfolder(path);
            return binFolder != null
                ? (binFolder.Parent == null ? binFolder.Name : binFolder.Parent.Name)
                : new DirectoryInfo(path).Name;
        }

        private static DirectoryInfo FindBinfolder(string path)
        {
            var binFolder = new DirectoryInfo(path);
            while (binFolder != null && !binFolder.Name.ToLower().Equals("bin"))
            {
                binFolder = binFolder.Parent;
            }

            return binFolder;
        }

        public void LogFallback(string msg, params object[] args)
        {
            if (_logLevel != ILogLevel.Off)
            {
                lock (_basicLockingTarget)
                {
                    try
                    {
                        var message =
                            (args != null && args.Any())
                                ? string.Format(msg, ObjArrayToStringArray(args))
                                : msg;

                        File.AppendAllLines(_fallbackLogPath, new string[] { message });
                    }
                    catch (Exception ex)
                    {
                        string message = "LogFallback: Problem with message parameter substitution.\n" + ex.Message;
                        File.AppendAllLines(_fallbackLogPath, new string[] { message, msg });
                    }
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
                LogFallback("Exception in the logging itself! {0}", ex.Message);
                exceptionLogged = true;
            }
        }
        private bool MessageParametersOK(string msg, params object[] pars)
        {
            try
            {
                string.Format(msg, pars);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Info(string msg, params object[] args)
        {
            var msg2 = string.Format("{0} - {1}", _callerClass, msg); // duh .. NLog ${callsite} does not work due to the wrapping
            try
            {
                //do not check the msg pars on each call for performace reasons
                _logger.Info(msg2, args);
            }
            catch (Exception ex)
            {
                bool msgParameterOk = !args.Any() || MessageParametersOK(msg2, args);

                if (msgParameterOk)
                {
                    // must be a logging problem
                    LogFallback(ex);
                    LogFallback(msg2);
                }
                else
                {
                    // parameter problem, probably nullpointer
                    // The logging can still have a problem, so rety without msg parameters
                    Info("Message parameter problem! " + msg);
                }
            }
        }

        public void Warn(string msg, params object[] args)
        {
            var msg2 = string.Format("{0} - {1}", _callerClass, msg); // duh .. NLog ${callsite} does not work due to the wrapping
            try
            {
                _logger.Warn(msg2, args);

            }
            catch (Exception ex)
            {
                bool msgParameterOk = !args.Any() || MessageParametersOK(msg2, args);

                if (msgParameterOk)
                {
                    //must be aloging problem
                    LogFallback(ex);
                    LogFallback(msg2);
                }
                else
                {
                    // parameter problem.
                    // The logging can still have a problem, so rety without msg parameters
                    Warn("Message parameter problem! " + msg);
                }
            }
        }

        public void Error(string msg, params object[] args)
        {
            var msg2 = string.Format("{0} - {1}", _callerClass, msg); // duh .. NLog ${callsite} does not work due to the wrapping
            try
            {
                _logger.Error(msg2, args);

            }
            catch (Exception ex)
            {
                bool msgParameterOk = !args.Any() || MessageParametersOK(msg2, args);

                if (msgParameterOk)
                {
                    //must be aloging problem
                    LogFallback(ex);
                    LogFallback(msg2);
                }
                else
                {
                    // parameter problem.
                    // The logging can still have a problem, so rety without msg parameters
                    Error("Message parameter problem! " + msg);
                }
            }
        }
 
        public void Debug(string msg, params object[] args)
        {
            var msg2 = string.Format("{0} - {1}", _callerClass, msg); // duh .. NLog ${callsite} does not work due to the wrapping
            try
            {
                _logger.Debug(msg2, args);

            }
            catch (Exception ex)
            {
                bool msgParameterOk = !args.Any() || MessageParametersOK(msg2, args);

                if (msgParameterOk)
                {
                    //must be aloging problem
                    LogFallback(ex);
                    LogFallback(msg2);
                }
                else
                {
                    // parameter problem.
                    // The logging can still have a problem, so rety without msg parameters
                    Debug("Message parameter problem! " + msg);
                }
            }
        }
        public void Trace(string msg, params object[] args)
        {
            var msg2 = string.Format("{0} - {1}", _callerClass, msg); // duh .. NLog ${callsite} does not work due to the wrapping
            try
            {
                _logger.Trace(msg2, args);

            }
            catch (Exception ex)
            {
                bool msgParameterOk = !args.Any() || MessageParametersOK(msg2, args);

                if (msgParameterOk)
                {
                    //must be aloging problem
                    LogFallback(ex);
                    LogFallback(msg2);
                }
                else
                {
                    // parameter problem.
                    // The logging can still have a problem, so rety without msg parameters
                    Trace("Message parameter problem! " + msg);
                }
            }
        }

        public void SetLevel(string level)
        {
            _logLevel = String2Enum(level);
        }
    }
}
