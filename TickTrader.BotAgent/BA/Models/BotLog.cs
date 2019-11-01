using System;
using System.Collections.Generic;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using NLog;
using NLog.Targets;
using NLog.Layouts;
using NLog.Config;
using System.IO;
using TickTrader.BotAgent.Extensions;
using System.Linq;
using TickTrader.Algo.Common.Model;
using ActorSharp;
using System.Threading.Tasks;

namespace TickTrader.BotAgent.BA.Models
{
    public class BotLog : Actor
    {
        private CircularList<ILogEntry> _logMessages;
        private ILogger _logger;
        private int _maxCachedRecords;
        private string _name;
        private string _logDirectory;
        private string _status;
        private readonly string _fileExtension = ".txt";
        private readonly string _archiveExtension = ".zip";

        private void Init(string name, int keepInMemory = 100)
        {
            _name = name;
            _logDirectory = Path.Combine(ServerModel.Environment.BotLogFolder, _name.Escape());
            _maxCachedRecords = keepInMemory;
            _logMessages = new CircularList<ILogEntry>(_maxCachedRecords);
            _logger = GetLogger(name);
        }

        public class ControlHandler : Handler<BotLog>
        {
            public ControlHandler(string name, int cacheSize = 100)
                : base(SpawnLocal<BotLog>(null, "BotLog: " + name))
            {
                Actor.Send(a => a.Init(name, cacheSize));
            }

            public Ref<BotLog> Ref => Actor;
            public IBotWriter GetWriter() => new LogWriter(Ref);
            public Task Clear() => Actor.Call(a => a.Clear());
        }

        public class Handler : BlockingHandler<BotLog>, IBotLog
        {
            public Handler(Ref<BotLog> logRef) : base(logRef) { }

            public IEnumerable<ILogEntry> Messages => CallActor(a => a._logMessages.ToArray());
            public string Status => CallActor(a => a._status);
            public string Folder => CallActor(a => a._logDirectory);
            public IFile[] Files => CallActor(a => a.GetFiles());

            public void Clear() => CallActor(a => a.Clear());
            public void DeleteFile(string file) => CallActor(a => a.DeleteFile(file));
            public IFile GetFile(string file) => CallActor(a => a.GetFile(file));
            public void SaveFile(string file, byte[] bytes) => throw new NotSupportedException("Saving files in bot logs folder is not allowed");
            public string GetFileReadPath(string file) => CallActor(a => a.GetFileReadPath(file));
            public string GetFileWritePath(string file) => throw new NotSupportedException("Writing files in bot logs folder is not allowed");

            public Task<string> GetStatusAsync() => CallActorAsync(a => a._status);
            public Task<List<ILogEntry>> QueryMessagesAsync(DateTime from, int maxCount) => CallActorAsync(a => a.QueryMessages(from, maxCount));
        }

        //public string Status { get; private set; }
        //public string Folder => _logDirectory;

        //public IEnumerable<ILogEntry> Messages
        //{
        //    get
        //    {
        //        return _logMessages.ToArray();
        //    }
        //}

        private List<ILogEntry> QueryMessages(DateTime from, int maxCount)
        {
            return _logMessages.Where(e => e.TimeUtc.Timestamp > from).Take(maxCount).ToList();
        }

        private IFile[] GetFiles()
        {
            if (Directory.Exists(_logDirectory))
            {
                DirectoryInfo dInfo = new DirectoryInfo(_logDirectory);
                return dInfo.GetFiles($"*{_fileExtension}").Select(fInfo => new ReadOnlyFileModel(fInfo.FullName)).ToArray();
            }
            else
                return new ReadOnlyFileModel[0];
        }

        //public event Action<string> StatusUpdated;

        private void WriteLog(LogEntryType type, string message)
        {
            var msg = new LogEntry(type, message);

            switch (type)
            {
                case LogEntryType.Custom:
                case LogEntryType.Info:
                case LogEntryType.Trading:
                case LogEntryType.TradingSuccess:
                case LogEntryType.TradingFail:
                case LogEntryType.Alert:
                    _logger.Info(msg.ToString());
                    break;
                case LogEntryType.Error:
                    _logger.Error(msg.ToString());
                    break;
            }

            if (IsLogFull)
                _logMessages.Dequeue();

            _logMessages.Add(msg);
        }

        private bool IsLogFull
        {
            get { return _logMessages.Count >= _maxCachedRecords; }
        }

        private ILogger GetLogger(string botId)
        {
            var logTarget = $"all-{botId}";
            var errTarget = $"error-{botId}";
            var statusTarget = $"status-{botId}";

            var logFile = new FileTarget(logTarget)
            {
                FileName = Layout.FromString(Path.Combine(_logDirectory, $"${{shortdate}}-log{_fileExtension}")),
                Layout = Layout.FromString("${longdate} | ${logger} | ${message}"),
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveFileName = Layout.FromString(Path.Combine(_logDirectory, $"{{#}}-log{_archiveExtension}")),
                ArchiveNumbering = ArchiveNumberingMode.Date,
                EnableArchiveFileCompression = true,
            };

            var errorFile = new FileTarget(errTarget)
            {
                FileName = Layout.FromString(Path.Combine(_logDirectory, $"${{shortdate}}-error{_fileExtension}")),
                Layout = Layout.FromString("${longdate} | ${logger} | ${message}"),
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveFileName = Layout.FromString(Path.Combine(_logDirectory, $"{{#}}-error{_archiveExtension}")),
                ArchiveNumbering = ArchiveNumberingMode.Date,
                EnableArchiveFileCompression = true,
            };

            var statusFile = new FileTarget(statusTarget)
            {
                FileName = Layout.FromString(Path.Combine(_logDirectory, $"${{shortdate}}-status{_fileExtension}")),
                Layout = Layout.FromString("${longdate} | ${logger} | ${message}"),
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveFileName = Layout.FromString(Path.Combine(_logDirectory, $"{{#}}-status{_archiveExtension}")),
                ArchiveNumbering = ArchiveNumberingMode.Date,
                EnableArchiveFileCompression = true,
            };

            var logConfig = new LoggingConfiguration();
            logConfig.AddTarget(logFile);
            logConfig.AddTarget(errorFile);
            logConfig.AddTarget(statusFile);
            logConfig.AddRule(LogLevel.Trace, LogLevel.Trace, statusTarget);
            logConfig.AddRule(LogLevel.Debug, LogLevel.Fatal, logTarget);
            logConfig.AddRule(LogLevel.Error, LogLevel.Fatal, errTarget);

            var nlogFactory = new LogFactory(logConfig);

            return nlogFactory.GetLogger(botId);
        }

        private IFile GetFile(string file)
        {
            if (file.IsFileNameValid())
            {
                var fullPath = Path.Combine(_logDirectory, file);

                return new ReadOnlyFileModel(fullPath);
            }

            throw new ArgumentException($"Incorrect file name {file}");
        }

        private string GetFileReadPath(string file)
        {
            if (file.IsFileNameValid())
            {
                var fullPath = Path.Combine(_logDirectory, file);

                return fullPath;
            }

            throw new ArgumentException($"Incorrect file name {file}");
        }

        private void Clear()
        {
            _logMessages.Clear();

            if (Directory.Exists(_logDirectory))
            {
                try
                {
                    new DirectoryInfo(_logDirectory).Clean();
                    Directory.Delete(_logDirectory);
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Could not clean log folder: " + _logDirectory);
                }
            }
        }

        private void DeleteFile(string file)
        {
            File.Delete(Path.Combine(_logDirectory, file));
        }

        private static LogEntryType Convert(LogSeverities severity)
        {
            switch (severity)
            {
                case LogSeverities.Custom: return LogEntryType.Custom;
                case LogSeverities.Error: return LogEntryType.Error;
                case LogSeverities.Info: return LogEntryType.Info;
                case LogSeverities.Trade: return LogEntryType.Trading;
                case LogSeverities.TradeSuccess: return LogEntryType.TradingSuccess;
                case LogSeverities.TradeFail: return LogEntryType.TradingFail;
                case LogSeverities.Alert: return LogEntryType.Alert;
                case LogSeverities.AlertClear: return LogEntryType.AlertClear;
                default: return LogEntryType.Info;
            }
        }

        private class LogWriter : BlockingHandler<BotLog>, IBotWriter
        {
            public LogWriter(Ref<BotLog> logRef) : base(logRef) { }

            public void LogMesssages(IEnumerable<PluginLogRecord> records)
            {
                CallActor(a =>
                {
                    foreach (var rec in records)
                    {
                        if (rec.Severity != LogSeverities.CustomStatus)
                            a.WriteLog(Convert(rec.Severity), rec.Message);
                    }
                });
            }

            public void Trace(string status) => CallActor(a => a._logger.Trace(status));
            public void UpdateStatus(string status) => CallActor(a => a._status = status);
        }
    }
}
