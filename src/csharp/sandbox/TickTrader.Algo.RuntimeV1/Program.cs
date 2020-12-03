using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.RuntimeV1
{
    class Program
    {
        static void Main(string[] args)
        {
            if (Debugger.IsAttached)
            {
                ConfigureNLog(Guid.NewGuid().ToString());
            }
            else
            {
                if (args.Length < 3)
                    Environment.FailFast(string.Join(", ", args));

                ConfigureNLog(args[2]);
            }

            CoreLoggerFactory.Init(n => new RuntimeLogAdapter(n));
            var logger = LogManager.GetLogger("MainLoop");

            logger.Info("Starting runtime with id {runtimeId} at server {address}:{port}", args[2], args[0], args[1]);

            RuntimeV1Loader loader = null;
            var isFailed = false;
            try
            {

                loader = new RuntimeV1Loader();
                loader.Init(args[0], int.Parse(args[1]), args[2]);
            }
            catch (Exception ex)
            {
                isFailed = true;
                logger.Error(ex, "Failed to init runtime. Aborting");
            }

            if (loader == null || isFailed)
                Environment.FailFast("Invalid start");

            loader.WhenFinished().Wait();
            logger.Info("Runtime finished.");

            try
            {
                loader.Deinit();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to deinit runtime.");
                Environment.FailFast("Failed to deinit runtime.");
            }
        }


        private static void ConfigureNLog(string runtimeId)
        {
            const string _fileExtension = ".log";
            const string _archiveExtension = ".zip";

            var _logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "RuntimeLogs", runtimeId);

            var logFile = new FileTarget("runtime-all")
            {
                FileName = Layout.FromString(Path.Combine(_logDirectory, $"${{shortdate}}-log{_fileExtension}")),
                Layout = Layout.FromString("${longdate} | ${logger} | ${message} | ${all-event-properties} ${exception:format=tostring}"),
                Encoding = Encoding.UTF8,
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveFileName = Layout.FromString(Path.Combine(_logDirectory, $"{{#}}-log{_archiveExtension}")),
                ArchiveNumbering = ArchiveNumberingMode.Date,
                EnableArchiveFileCompression = true,
            };

            var errorFile = new FileTarget("runtime-error")
            {
                FileName = Layout.FromString(Path.Combine(_logDirectory, $"${{shortdate}}-error{_fileExtension}")),
                Layout = Layout.FromString("${longdate} | ${logger} | ${message} | ${all-event-properties} ${exception:format=tostring}"),
                Encoding = Encoding.UTF8,
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveFileName = Layout.FromString(Path.Combine(_logDirectory, $"{{#}}-error{_archiveExtension}")),
                ArchiveNumbering = ArchiveNumberingMode.Date,
                EnableArchiveFileCompression = true,
            };

            var logConfig = new LoggingConfiguration();
            logConfig.AddTarget(logFile);
            logConfig.AddTarget(errorFile);
            logConfig.AddRule(LogLevel.Debug, LogLevel.Fatal, logFile);
            logConfig.AddRule(LogLevel.Error, LogLevel.Fatal, errorFile);

            LogManager.Configuration = logConfig;
        }


        internal class RuntimeLogAdapter : IAlgoCoreLogger
        {
            private readonly Logger _logger;

            public RuntimeLogAdapter(string componentName)
            {
                _logger = LogManager.GetLogger(componentName);
            }

            public void Debug(string msg)
            {
                _logger.Debug(msg);
            }

            public void Debug(string msgFormat, params object[] msgParams)
            {
                _logger.Debug(msgFormat, msgParams);
            }

            public void Error(string msg)
            {
                _logger.Error(msg);
            }

            public void Error(Exception ex)
            {
                _logger.Error(ex);
            }

            public void Error(string msg, Exception ex)
            {
                _logger.Error(ex, msg);
            }

            public void Error(Exception ex, string msgFormat, params object[] msgParams)
            {
                _logger.Error(ex, msgFormat, msgParams);
            }

            public void Info(string msg)
            {
                _logger.Info(msg);
            }

            public void Info(string msgFormat, params object[] msgParams)
            {
                _logger.Info(msgFormat, msgParams);
            }
        }
    }
}
