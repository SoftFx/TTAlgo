using NLog;
using NLog.Config;
using NLog.LayoutRenderers;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System.IO;
using System.Text;

namespace TickTrader.Algo.Server.Common
{
    public static class LoggerHelper
    {
        public const string SessionLoggerPrefix = "Session.";
        public const string FileExtension = ".txt";
        public const string ArchiveExtension = ".zip";
        public const string LogLayout = "${longdate} | ${message} ${onexception:${newline}${exception:format=toString,Data:maxInnerExceptionLevel=10}}";


        static LoggerHelper()
        {
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition("sessionId", typeof(SessionIdLayoutRenderer));
        }


        public static ILogger GetSessionLogger(LogFactory logFactory, string sessionId)
        {
            return logFactory.GetLogger($"{SessionLoggerPrefix}{sessionId}");
        }

        public static LoggingConfiguration CreateClientConfig(string logsDir, string folderName)
        {
            var logConfig = new LoggingConfiguration();
            var p = new NLogFileParams { LogDirectory = Path.Combine(logsDir, folderName), Layout = LogLayout };

            p.FileNameSuffix = "log";
            logConfig.AddRule(LogLevel.Trace, LogLevel.Fatal, CreateAsyncFileTarget(p, 100, 1000));

            p.FileNameSuffix = "error";
            logConfig.AddRule(LogLevel.Error, LogLevel.Fatal, CreateAsyncFileTarget(p, 20, 100));

            return logConfig;
        }

        public static LoggingConfiguration CreateServerConfig(string logsDir, string folderName)
        {
            var logConfig = new LoggingConfiguration();
            var p = new NLogFileParams { LogDirectory = Path.Combine(logsDir, folderName), Layout = LogLayout };

            p.FileNameSuffix = "log";
            logConfig.AddRule(LogLevel.Trace, LogLevel.Fatal, CreateAsyncFileTarget(p, 100, 1000));

            p.FileNameSuffix = "error";
            logConfig.AddRule(LogLevel.Error, LogLevel.Fatal, CreateAsyncFileTarget(p, 20, 100));

            p.LogDirectory = Path.Combine(p.LogDirectory, "Sessions");
            p.SkipDate = true;
            p.FileNameSuffix = $"${{sessionId}}{FileExtension}";
            logConfig.AddRule(LogLevel.Trace, LogLevel.Fatal, CreateAsyncFileTarget(p, 100, 1000), string.Concat(SessionLoggerPrefix, "*"));

            return logConfig;
        }

        public static ILogger GetLogger(string loggerName, string logsDir, string folderName)
        {
            var logConfig = CreateServerConfig(logsDir, folderName);

            var nlogFactory = new LogFactory(logConfig);

            return nlogFactory.GetLogger(loggerName);
        }


        internal static FileTarget CreateFileTarget(NLogFileParams p)
        {
            return new FileTarget
            {
                FileName = p.SkipDate
                    ? Layout.FromString(Path.Combine(p.LogDirectory, $"{p.FileNameSuffix}{FileExtension}"))
                    : Layout.FromString(Path.Combine(p.LogDirectory, $"${{shortdate}}-{p.FileNameSuffix}{FileExtension}")),
                Layout = Layout.FromString(p.Layout),
                Encoding = Encoding.UTF8,
            };
        }

        internal static AsyncTargetWrapper CreateAsyncFileTarget(NLogFileParams p, int batchSize, int queueLimit,
            AsyncTargetWrapperOverflowAction overflowAction = AsyncTargetWrapperOverflowAction.Block)
        {
            return new AsyncTargetWrapper(CreateFileTarget(p))
            {
                BatchSize = batchSize,
                QueueLimit = queueLimit,
                OverflowAction = overflowAction,
            };
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

    internal struct NLogFileParams
    {
        public string LogDirectory { get; set; }

        public string FileNameSuffix { get; set; }

        public string Layout { get; set; }

        public bool SkipDate { get; set; }
    }
}
