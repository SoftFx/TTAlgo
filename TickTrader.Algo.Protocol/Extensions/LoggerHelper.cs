using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using System.IO;

namespace TickTrader.Algo.Protocol
{
    internal static class LoggerHelper
    {
        internal static ILogger GetLogger(string loggerName, string logsDir, string folderName)
        {
            logsDir = Path.Combine(logsDir, folderName);

            var logTarget = $"all-{folderName}";
            var errTarget = $"error-{folderName}";

            var logFile = new FileTarget(logTarget)
            {
                FileName = Layout.FromString(Path.Combine(logsDir, folderName, "${{shortdate}}-log.txt")),
                Layout = Layout.FromString("${longdate} | ${message}")
            };

            var errorFile = new FileTarget(errTarget)
            {
                FileName = Layout.FromString(Path.Combine(logsDir, folderName, "${{shortdate}}-error.txt")),
                Layout = Layout.FromString("${longdate} | ${message}"),
            };

            var logConfig = new LoggingConfiguration();
            logConfig.AddTarget(logFile);
            logConfig.AddTarget(errorFile);
            logConfig.AddRule(LogLevel.Trace, LogLevel.Fatal, logTarget);
            logConfig.AddRule(LogLevel.Error, LogLevel.Fatal, errTarget);

            var nlogFactory = new LogFactory(logConfig);

            return nlogFactory.GetLogger(loggerName);
        }
    }
}
