using System;
using System.Collections.Generic;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using NLog;
using NLog.Targets;
using NLog.Layouts;
using NLog.Config;
using System.IO;
using TickTrader.DedicatedServer.Extensions;
using System.Linq;

namespace TickTrader.DedicatedServer.DS.Models
{
    public class BotLog : CrossDomainObject, IPluginLogger, IBotLog
    {
        private object _internalSync = new object();
        private object _sync;
        private List<ILogEntry> _logMessages;
        private ILogger _logger;
        private int _keepInmemory;
        private string _name;
        private string _logDirectory;
        private readonly string _fileExtension = ".txt";

        public BotLog(string name, object sync, int keepInMemory = 100)
        {
            _sync = sync;
            _name = name;
            _logDirectory = $"{ServerModel.Environment.BotLogFolder}/{_name.Escape()}/";
            _keepInmemory = keepInMemory;
            _logMessages = new List<ILogEntry>(_keepInmemory);
            _logger = GetLogger(name);
        }

        public string Status { get; private set; }

        public IEnumerable<ILogEntry> Messages
        {
            get
            {
                lock (_sync)
                    return _logMessages.ToArray();
            }
        }

        public IFile[] Files
        {
            get
            {
                if (Directory.Exists(_logDirectory))
                {
                    DirectoryInfo dInfo = new DirectoryInfo(_logDirectory);
                    return dInfo.GetFiles($"*{_fileExtension}").Select(fInfo => new ReadOnlyFileModel(fInfo.FullName)).ToArray();
                }
                else
                    return new ReadOnlyFileModel[0];
            }
        }

        public event Action<string> StatusUpdated;

        public void OnError(Exception ex)
        {
            WriteLog(LogEntryType.Error, ex.Message);
        }

        public void OnExit()
        {
            WriteLog(LogEntryType.Info, "Exit");
        }

        public void OnInitialized()
        {
            WriteLog(LogEntryType.Info, "Initialized");
        }

        public void OnPrint(string message)
        {
            WriteLog(LogEntryType.Custom, message);
        }

        public void OnPrint(string message, params object[] parameters)
        {
            OnPrint(string.Format(message, parameters));
        }

        public void OnPrintError(string message)
        {
            WriteLog(LogEntryType.Error, message);
        }

        public void OnPrintError(string message, params object[] parameters)
        {
            OnPrintError(string.Format(message, parameters));
        }

        public void OnPrintInfo(string message)
        {
            WriteLog(LogEntryType.Info, message);
        }

        public void OnPrintTrade(string message)
        {
            WriteLog(LogEntryType.Trading, message);
        }

        public void OnStart()
        {
            WriteLog(LogEntryType.Info, "Start");
        }

        public void OnStop()
        {
            WriteLog(LogEntryType.Info, "Stop");
        }

        public void UpdateStatus(string status)
        {
            lock (_sync)
            {
                Status = status;
                StatusUpdated?.Invoke(status);
            }
        }

        private void WriteLog(LogEntryType type, string message)
        {
            lock (_internalSync)
            {
                var msg = new LogEntry(type, message);

                switch (type)
                {
                    case LogEntryType.Custom:
                    case LogEntryType.Info:
                    case LogEntryType.Trading:
                        _logger.Info(msg.ToString());
                        break;
                    case LogEntryType.Error:
                        _logger.Error(msg.ToString());
                        break;
                }

                if (_logMessages.Count >= _keepInmemory)
                    _logMessages.RemoveAt(0);

                _logMessages.Add(msg);
            }
        }

        private ILogger GetLogger(string botId)
        {
            var logTarget = $"all-{botId}";
            var errTarget = $"error-{botId}";

            var logFile = new FileTarget(logTarget)
            {
                FileName = Layout.FromString($"{_logDirectory}${{shortdate}}-log{_fileExtension}"),
                Layout = Layout.FromString("${longdate} | ${logger} | ${message}")
            };

            var errorFile = new FileTarget(errTarget)
            {
                FileName = Layout.FromString($"{_logDirectory}${{shortdate}}-error{_fileExtension}"),
                Layout = Layout.FromString("${longdate} | ${logger} | ${message}")
            };

            var logConfig = new LoggingConfiguration();
            logConfig.AddTarget(logFile);
            logConfig.AddTarget(errorFile);
            logConfig.AddRule(LogLevel.Trace, LogLevel.Fatal, logTarget);
            logConfig.AddRule(LogLevel.Error, LogLevel.Fatal, errTarget);

            var nlogFactory = new LogFactory(logConfig);

            return nlogFactory.GetLogger(botId);
        }

        public IFile GetFile(string file)
        {
            if (file.IsFileNameValid())
            {
                var fullPath = Path.Combine(_logDirectory, file);

                return new ReadOnlyFileModel(fullPath);
            }

            throw new ArgumentException($"Incorrect file name {file}");
        }

        public void Clean()
        {
            lock (_sync)
            {
                _logMessages.Clear();

                if (Directory.Exists(_logDirectory))
                {
                    new DirectoryInfo(_logDirectory).Clean();
                    Directory.Delete(_logDirectory);
                }
            }
        }

        private void CleanFolder(string logDirectory)
        {
            var dir = new DirectoryInfo(logDirectory);

            foreach (FileInfo fi in dir.GetFiles())
            {
                fi.Delete();
            }

            foreach (DirectoryInfo di in dir.GetDirectories())
            {
                CleanFolder(di.FullName);
                di.Delete();
            }
        }

        public void DeleteFile(string file)
        {
            File.Delete(Path.Combine(_logDirectory, file));
        }
    }
}
