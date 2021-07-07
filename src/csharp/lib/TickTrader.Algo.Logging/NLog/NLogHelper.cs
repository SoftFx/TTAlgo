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

            var p = new NLogFileParams { LogDirectory = logDir, Layout = "${longdate} | ${message}" };

            var allTarget = "plugin-all";
            p.TargetName = allTarget;
            p.FileNameSuffix = "all";
            logConfig.AddTarget(CreateAsyncFileTarget(p, 100, 1000));
            logConfig.AddRule(LogLevel.Debug, LogLevel.Fatal, allTarget);

            var errTarget = "plugin-error";
            p.TargetName = errTarget;
            p.FileNameSuffix = "error";
            logConfig.AddTarget(CreateAsyncFileTarget(p, 20, 100));
            logConfig.AddRule(LogLevel.Error, LogLevel.Fatal, errTarget);

            var statusTarget = "plugin-status";
            p.TargetName = statusTarget;
            p.FileNameSuffix = "status";
            logConfig.AddTarget(CreateAsyncFileTarget(p, 20, 100));
            logConfig.AddRule(LogLevel.Trace, LogLevel.Trace, statusTarget);

            return logConfig;
        }

        public static LoggingConfiguration CreateRuntimeConfig(string logDir)
        {
            var logConfig = new LoggingConfiguration();

            var p = new NLogFileParams { LogDirectory = logDir, Layout = "${longdate} | ${level} | ${logger} | ${message} ${exception:format=tostring}" };

            var allTarget = "runtime-all";
            p.TargetName = allTarget;
            p.FileNameSuffix = "all";
            logConfig.AddTarget(CreateAsyncFileTarget(p, 500, 10000));
            logConfig.AddRule(LogLevel.Debug, LogLevel.Fatal, allTarget);

            var errTarget = "runtime-error";
            p.TargetName = errTarget;
            p.FileNameSuffix = "error";
            logConfig.AddTarget(CreateAsyncFileTarget(p, 50, 1000));
            logConfig.AddRule(LogLevel.Error, LogLevel.Fatal, errTarget);

            return logConfig;
        }
    }
}
