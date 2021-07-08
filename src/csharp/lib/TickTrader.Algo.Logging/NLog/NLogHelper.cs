using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System.IO;
using System.Text;

namespace TickTrader.Algo.Logging
{
    public struct NLogFileParams
    {
        public string TargetName { get; set; }

        public string LogDirectory { get; set; }

        public string FileNameSuffix { get; set; }

        public string Layout { get; set; }
    }


    public static class NLogHelper
    {
        public const string FileExtension = ".log";
        public const string ArchiveExtension = ".zip";
        public const string SimpleLogLayout = "${longdate} | ${message}";
        public const string NormalLogLayout = "${longdate} | ${level} | ${logger} | ${message} ${exception:format=tostring}";


        public static FileTarget CreateFileTarget(NLogFileParams p)
        {
            return new FileTarget(p.TargetName)
            {
                FileName = Layout.FromString(Path.Combine(p.LogDirectory, $"${{shortdate}}-{p.FileNameSuffix}{FileExtension}")),
                Layout = Layout.FromString(p.Layout),
                Encoding = Encoding.UTF8,
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveFileName = Layout.FromString(Path.Combine(p.LogDirectory, $"{{#}}-{p.FileNameSuffix}{ArchiveExtension}")),
                ArchiveNumbering = ArchiveNumberingMode.Date,
                EnableArchiveFileCompression = true,
            };
        }

        public static AsyncTargetWrapper CreateAsyncFileTarget(NLogFileParams p, int batchSize, int queueLimit,
            AsyncTargetWrapperOverflowAction overflowAction = AsyncTargetWrapperOverflowAction.Block)
        {
            return new AsyncTargetWrapper(CreateFileTarget(p))
            {
                Name = p.TargetName,
                BatchSize = batchSize,
                QueueLimit = queueLimit,
                OverflowAction = overflowAction,
            };
        }


        public static LoggingConfiguration CreatePluginConfig(string logDir)
        {
            var logConfig = new LoggingConfiguration();

            var p = new NLogFileParams { LogDirectory = logDir, Layout = SimpleLogLayout };

            p.FileNameSuffix = "all";
            logConfig.AddRule(LogLevel.Debug, LogLevel.Fatal, CreateAsyncFileTarget(p, 100, 1000));

            p.FileNameSuffix = "error";
            logConfig.AddRule(LogLevel.Error, LogLevel.Fatal, CreateAsyncFileTarget(p, 20, 100));

            p.FileNameSuffix = "status";
            logConfig.AddRule(LogLevel.Trace, LogLevel.Trace, CreateAsyncFileTarget(p, 20, 100));

            return logConfig;
        }

        public static LoggingConfiguration CreateRuntimeConfig(string logDir)
        {
            var logConfig = new LoggingConfiguration();

            var p = new NLogFileParams { LogDirectory = logDir, Layout = NormalLogLayout };

            p.FileNameSuffix = "all";
            logConfig.AddRule(LogLevel.Debug, LogLevel.Fatal, CreateAsyncFileTarget(p, 500, 10000));

            p.FileNameSuffix = "error";
            logConfig.AddRule(LogLevel.Error, LogLevel.Fatal, CreateAsyncFileTarget(p, 200, 1000));

            return logConfig;
        }
    }
}
