-----------------------------------------------------------
-------------- README NLog.Wrapper.ZeroConfig -------------
-----------------------------------------------------------
This wrapper for NLog v 4.3.11 will provide zero config logging in a logs subfolder of your executable or website root.
This will work in web as well as windows ENV.
If this folder is not accesible, e.g. due to rights, log messages will end up in c:\temp

The code is no big deal, AND PROBABLY NOT COMPLETE ;) https://github.com/it-arjan/NLog.ZeroConfig
Usage example:

using NLogWrapper;
ILogger _logger = LogManager.CreateLogger(typeof(yourClass));