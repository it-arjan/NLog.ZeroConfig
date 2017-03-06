-----------------------------------------------------------
-------------- README NLog.Wrapper.ZeroConfig -------------
-----------------------------------------------------------
This wrapper for NLog v 4.3.11 will provide zero config logging in a /logs subfolder of your executable or website root.
This will work in Web & Windows ENV.

The code is no big deal, AND PROBABLY NOT COMPLETE ;) https://github.com/it-arjan/NLog.ZeroConfig

--------
-- Version 1.1 --
------------
Usage example:

using NLogWrapper;
ILogger _logger = LogManager.CreateLogger(typeof(yourClass)); 
// Log level will be Debug
// Creates modulePath/logs or wwroot/logs
// fallback log folder = Path.GetTempfolder()

ILogger _logger = LogManager.CreateLogger(typeof(yourClass), ILogLevel.Error)
// allows to set log level


ILogger _logger = LogManager.CreateLogger(typeof(yourClass), ILogLevel.Error, "logging-fallback-folder-must-exist")
// allows to set fallback log path

--------
-- Version 1.0 --
------------

using NLogWrapper;
ILogger _logger = LogManager.CreateLogger(typeof(yourClass));  
