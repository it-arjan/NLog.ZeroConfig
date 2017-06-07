-----------------------------------------------------------
-------------- README NLog.Wrapper.ZeroConfig -------------
-----------------------------------------------------------
This wrapper for NLog v 4.3.11 will provide zero config logging in a /logs subfolder of your executable or website root.
This will work in Web & Windows ENV.

The code is no big deal, AND PROBABLY NOT COMPLETE ;) https://github.com/it-arjan/NLog.ZeroConfig

--------
-- Version 1.1.5 --
------------
Usage example:

using NLogWrapper;

// Log level will be Debug
// Creates modulePath/logs or wwroot/logs
// fallback log folder = Path.GetTempfolder()
ILogger _logger = LogManager.CreateLogger(typeof(yourClass)); 

// Create with specific log level
ILogger _logger = LogManager.CreateLogger(typeof(yourClass), "Error")

// since 1.1.5, set log level after creation, casing does not matter
_logger.SetLevel("Error")

// allows to set fallback log path
ILogger _logger = LogManager.CreateLogger(typeof(yourClass), ILogLevel.Error, "logging-fallback-folder-must-exist")

_logger.Debug("The focus is on {0} logging and on {1}", "RELIABLE", "convenience");
_logger.Debug("a mistake in message parameter numbering will NOT throw an Exception, but produces an error message in log file. {1}", "Like this");

--------
-- Version 1.0 --
------------

using NLogWrapper;
ILogger _logger = LogManager.CreateLogger(typeof(yourClass));  
