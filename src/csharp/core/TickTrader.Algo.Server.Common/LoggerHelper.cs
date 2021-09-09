using NLog;
using NLog.Config;
using NLog.LayoutRenderers;
using NLog.Layouts;
using NLog.Targets;
using System.IO;
using System.Text;

namespace TickTrader.Algo.Server.Common
{
    public static class LoggerHelper
    {
        public const string SessionLoggerPrefix = "Session.";


        static LoggerHelper()
        {
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition("sessionId", typeof(SessionIdLayoutRenderer));
        }


        public static ILogger GetSessionLogger(LogFactory logFactory, string sessionId)
        {
            return logFactory.GetLogger($"{SessionLoggerPrefix}{sessionId}");
        }
        
        public static ILogger GetLogger(string loggerName, string logsDir, string folderName)
        {
            var logTargetName = $"all-{folderName}";
            var errorTargetName = $"error-{folderName}";
            var sessionTargetName = $"session-{folderName}";

            var logTarget = new FileTarget(logTargetName)
            {
                FileName = Layout.FromString(Path.Combine(logsDir, folderName, "${shortdate}-log.txt")),
                Layout = Layout.FromString("${longdate} | ${message} ${onexception:${newline}${exception:format=toString,Data:maxInnerExceptionLevel=10}}")
            };

            var errorTarget = new FileTarget(errorTargetName)
            {
                FileName = Layout.FromString(Path.Combine(logsDir, folderName, "${shortdate}-error.txt")),
                Layout = Layout.FromString("${longdate} | ${message} ${onexception:${newline}${exception:format=toString,Data:maxInnerExceptionLevel=10}}"),
            };

            var sessionTarget = new FileTarget(sessionTargetName)
            {
                FileName = Layout.FromString(Path.Combine(logsDir, folderName, "Sessions", "${sessionId}.txt")),
                Layout = Layout.FromString("${longdate} | ${message} ${onexception:${newline}${exception:format=toString,Data:maxInnerExceptionLevel=10}}")
            };

            var logRule = new LoggingRule("*", LogLevel.Trace, LogLevel.Fatal, logTarget);
            var errorRule = new LoggingRule("*", LogLevel.Error, LogLevel.Fatal, errorTarget);
            var sessionRule = new LoggingRule(string.Concat(SessionLoggerPrefix, "*"), LogLevel.Trace, LogLevel.Fatal, sessionTarget) { Final = true };

            var logConfig = new LoggingConfiguration();
            logConfig.LoggingRules.Add(sessionRule);
            logConfig.LoggingRules.Add(errorRule);
            logConfig.LoggingRules.Add(logRule);

            var nlogFactory = new LogFactory(logConfig);

            return nlogFactory.GetLogger(loggerName);
        }
    }

    internal class SessionIdLayoutRenderer : LayoutRenderer
    {
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (logEvent.LoggerName.StartsWith(LoggerHelper.SessionLoggerPrefix))
                builder.Append(logEvent.LoggerName.Substring(LoggerHelper.SessionLoggerPrefix.Length));
            else
                builder.Append(logEvent.LoggerName);
        }
    }
}
