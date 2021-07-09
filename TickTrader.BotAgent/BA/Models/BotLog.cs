using System;
using System.Collections.Generic;
using TickTrader.Algo.Core.Lib;
using System.IO;
using TickTrader.BotAgent.Extensions;
using System.Linq;
using ActorSharp;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using TickTrader.Algo.Domain;

namespace TickTrader.BotAgent.BA.Models
{
    public class BotLog : Actor
    {
        private CircularList<ILogEntry> _logMessages;
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
        }

        public class ControlHandler : Handler<BotLog>
        {
            public ControlHandler(string name, AlertStorage storage, int cacheSize = 100)
                : base(SpawnLocal<BotLog>(null, "BotLog: " + name))
            {
                Actor.Send(a => a.Init(name, storage, cacheSize));
            }

            public Ref<BotLog> Ref => Actor;
            public Task<string> GetFolder() => Actor.Call(a => a._logDirectory);
            public Task Clear() => Actor.Call(a => a.Clear());
            internal void AddLog(PluginLogRecord r) => Actor.Send(a => a.WriteLog(r));
            internal void UpdateStatus(PluginStatusUpdate update) => Actor.Send(a => a._status = update.Message);
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

        private void WriteLog(PluginLogRecord record)
        {
            var msg = new LogEntry(record);

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
    }
}
