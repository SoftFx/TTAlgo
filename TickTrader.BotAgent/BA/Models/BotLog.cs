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
using System.Text;
using Google.Protobuf.WellKnownTypes;
using TickTrader.Algo.Domain;
using NLog.Targets.Wrappers;
using System.Threading.Channels;

namespace TickTrader.BotAgent.BA.Models
{
    public class BotLog : Actor
    {
        private CircularList<ILogEntry> _logMessages;
        private ILogger _logger;
        private AlertStorage _alertStorage;
        private int _maxCachedRecords;
        private string _name;
        private string _logDirectory;
        private string _status;
        private readonly string _fileExtension = ".txt";
        private readonly string _archiveExtension = ".zip";

        private void Init(string name, AlertStorage storage, int keepInMemory = 100)
        {
            _name = name;
            _logDirectory = Path.Combine(ServerModel.Environment.BotLogFolder, _name.Escape());
            _alertStorage = storage;
            _maxCachedRecords = keepInMemory;
            _logMessages = new CircularList<ILogEntry>(_maxCachedRecords);
            _logger = GetLogger(name);
        }

        public class ControlHandler : Handler<BotLog>
        {
            public ControlHandler(string name, AlertStorage storage, int cacheSize = 100)
                : base(SpawnLocal<BotLog>(null, "BotLog: " + name))
            {
                Actor.Send(a => a.Init(name, storage, cacheSize));
            }

            public Ref<BotLog> Ref => Actor;
            public IBotWriter GetWriter() => Actor.Call(a => new LogWriter(a)).Result;
            public Task Clear() => Actor.Call(a => a.Clear());
        }

        public class Handler : BlockingHandler<BotLog>, IBotLog
        {
            public Handler(Ref<BotLog> logRef) : base(logRef) { }

            public Task<string> GetFolder() => CallActorAsync(a => a._logDirectory);
            public Task<IFile[]> GetFiles() => CallActorAsync(a => a.GetFiles());
            public Task Clear() => CallActorAsync(a => a.Clear());
            public Task DeleteFile(string file) => CallActorAsync(a => a.DeleteFile(file));
            public Task<IFile> GetFile(string file) => CallActorAsync(a => a.GetFile(file));
            public Task SaveFile(string file, byte[] bytes) => throw new NotSupportedException("Saving files in bot logs folder is not allowed");
            public Task<string> GetFileReadPath(string file) => CallActorAsync(a => a.GetFileReadPath(file));
            public Task<string> GetFileWritePath(string file) => throw new NotSupportedException("Writing files in bot logs folder is not allowed");

            public Task<ILogEntry[]> GetMessages() => CallActorAsync(a => a._logMessages.ToArray());
            public Task<string> GetStatusAsync() => CallActorAsync(a => a._status);
            public Task<List<ILogEntry>> QueryMessagesAsync(Timestamp from, int maxCount) => CallActorAsync(a => a.QueryMessages(from, maxCount));
        }


        private List<ILogEntry> QueryMessages(Timestamp from, int maxCount)
        {
            return _logMessages.Where(e => e.TimeUtc > from).Take(maxCount).ToList();
        }

        private IFile[] GetFiles()
        {
            if (Directory.Exists(_logDirectory))
            {
                DirectoryInfo dInfo = new DirectoryInfo(_logDirectory);
                return dInfo.GetFiles($"*{_fileExtension}").Select(fInfo => new ReadOnlyFileModel(fInfo.FullName))
                    .Concat(dInfo.GetFiles($"*{_archiveExtension}").Select(fInfo => new ReadOnlyFileModel(fInfo.FullName))).ToArray();
            }
            else
                return new ReadOnlyFileModel[0];
        }

        //public event Action<string> StatusUpdated;

        private void WriteLog(PluginLogRecord record)
        {
            var msg = new LogEntry(record);

            switch (record.Severity)
            {
                case PluginLogRecord.Types.LogSeverity.Custom:
                case PluginLogRecord.Types.LogSeverity.Info:
                case PluginLogRecord.Types.LogSeverity.Trade:
                case PluginLogRecord.Types.LogSeverity.TradeSuccess:
                case PluginLogRecord.Types.LogSeverity.TradeFail:
                    _logger.Info(msg.ToString());
                    break;
                case PluginLogRecord.Types.LogSeverity.Alert:
                    _logger.Warn(msg.ToString());
                    break;
                case PluginLogRecord.Types.LogSeverity.Error:
                    _logger.Error(msg.ToString());
                    if (!string.IsNullOrEmpty(record.Details))
                        _logger.Error(record.Details);
                    break;
            }

            if (IsLogFull)
                _logMessages.Dequeue();

            _logMessages.Add(msg);

            if (record.Severity == PluginLogRecord.Types.LogSeverity.Alert)
                _alertStorage.AddAlert(msg, _name);
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
                Encoding = Encoding.UTF8,
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveFileName = Layout.FromString(Path.Combine(_logDirectory, $"{{#}}-log{_archiveExtension}")),
                ArchiveNumbering = ArchiveNumberingMode.Date,
                EnableArchiveFileCompression = true,
            };

            var errorFile = new FileTarget(errTarget)
            {
                FileName = Layout.FromString(Path.Combine(_logDirectory, $"${{shortdate}}-error{_fileExtension}")),
                Layout = Layout.FromString("${longdate} | ${logger} | ${message}"),
                Encoding = Encoding.UTF8,
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveFileName = Layout.FromString(Path.Combine(_logDirectory, $"{{#}}-error{_archiveExtension}")),
                ArchiveNumbering = ArchiveNumberingMode.Date,
                EnableArchiveFileCompression = true,
            };

            var statusFile = new FileTarget(statusTarget)
            {
                FileName = Layout.FromString(Path.Combine(_logDirectory, $"${{shortdate}}-status{_fileExtension}")),
                Layout = Layout.FromString("${longdate} | ${logger} | ${message}"),
                Encoding = Encoding.UTF8,
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveFileName = Layout.FromString(Path.Combine(_logDirectory, $"{{#}}-status{_archiveExtension}")),
                ArchiveNumbering = ArchiveNumberingMode.Date,
                EnableArchiveFileCompression = true,
            };

            var logWrapper = new AsyncTargetWrapper(logFile)
            {
                Name = logTarget,
                BatchSize = 100,
                QueueLimit = 1000,
                OverflowAction = AsyncTargetWrapperOverflowAction.Block,
            };

            var errorWrapper = new AsyncTargetWrapper(errorFile)
            {
                Name = errTarget,
                BatchSize = 20,
                QueueLimit = 100,
                OverflowAction = AsyncTargetWrapperOverflowAction.Block,
            };

            var statusWrapper = new AsyncTargetWrapper(statusFile)
            {
                Name = statusTarget,
                BatchSize = 20,
                QueueLimit = 100,
                OverflowAction = AsyncTargetWrapperOverflowAction.Block,
            };

            var logConfig = new LoggingConfiguration();
            logConfig.AddTarget(logWrapper);
            logConfig.AddTarget(errorWrapper);
            logConfig.AddTarget(statusWrapper);
            logConfig.AddRule(LogLevel.Trace, LogLevel.Trace, statusTarget);
            logConfig.AddRule(LogLevel.Debug, LogLevel.Fatal, logTarget);
            logConfig.AddRule(LogLevel.Error, LogLevel.Fatal, errTarget);

            _alertStorage.AttachedAlertLogger(logConfig, botId, _logDirectory, _fileExtension, _archiveExtension);

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
                    throw new Exception("Could not clean log folder: " + _logDirectory, ex);
                }
            }
        }

        private void DeleteFile(string file)
        {
            File.Delete(Path.Combine(_logDirectory, file));
        }


        private class LogWriter : IBotWriter
        {
            private const int MaxBatchSize = 500;

            private BotLog _log;
            private System.Threading.Channels.Channel<Action<BotLog>> _channel;

            public LogWriter(BotLog log)
            {
                _log = log;

                var options = new BoundedChannelOptions(1000)
                {
                    AllowSynchronousContinuations = false,
                    FullMode = BoundedChannelFullMode.Wait,
                    SingleReader = true,
                };

                _channel = System.Threading.Channels.Channel.CreateBounded<Action<BotLog>>(options);
                var t = ReadLoop();
            }

            public void LogMesssage(PluginLogRecord record)
            {
                _channel.Writer.TryWrite(a => a.WriteLog(record));
            }

            public void Trace(string status) => _channel.Writer.TryWrite(a => a._logger.Trace(status));
            public void UpdateStatus(string status) => _channel.Writer.TryWrite(a => a._status = status);

            public void Close()
            {
                _channel.Writer.Complete();
            }

            private async Task ReadLoop()
            {
                var reader = _channel.Reader;

                while (await reader.WaitToReadAsync())
                {
                    var cnt = 0;
                    while (reader.TryRead(out var action) && cnt < MaxBatchSize)
                    {
                        action(_log);
                        cnt++;
                    }
                }
            }
        }
    }
}
