using System;
using System.Collections.Generic;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using NLog;
using NLog.Targets;
using NLog.Layouts;
using NLog.Config;
using TickTrader.Algo.Common.Lib;

namespace TickTrader.DedicatedServer.DS.Models
{
    public class BotLog : CrossDomainObject, IPluginLogger, IBotLog
    {
        private object _internalSync = new object();
        private object _sync;
        private List<ILogMessage> _logMessages;
        private ILogger _logger;
        private int _keepInmemory;
        private string _name;

        public BotLog(string name, object sync, int keepInMemory = 100)
        {
            _sync = sync;
            _name = name;
            _keepInmemory = keepInMemory;
            _logMessages = new List<ILogMessage>(_keepInmemory);
            _logger = GetLogger(name);
        }

        public string Status { get; private set; }

        public IEnumerable<ILogMessage> Messages
        {
            get
            {
                lock (_internalSync)
                    return _logMessages.ToArray();
            }
        }

        public event Action<string> StatusUpdated;

        public void OnError(Exception ex)
        {
            WriteLog(LogMessageType.Error, ex.Message);
        }

        public void OnExit()
        {
            WriteLog(LogMessageType.Info, "Exit");
        }

        public void OnInitialized()
        {
            WriteLog(LogMessageType.Info, "Initialized");
        }

        public void OnPrint(string message)
        {
            WriteLog(LogMessageType.Custom, message);
        }

        public void OnPrint(string message, params object[] parameters)
        {
            OnPrint(string.Format(message, parameters));
        }

        public void OnPrintError(string message)
        {
            WriteLog(LogMessageType.Error, message);
        }

        public void OnPrintError(string message, params object[] parameters)
        {
            OnPrintError(string.Format(message, parameters));
        }

        public void OnPrintInfo(string message)
        {
            WriteLog(LogMessageType.Info, message);
        }

        public void OnPrintTrade(string message)
        {
            WriteLog(LogMessageType.Trading, message);
        }

        public void OnStart()
        {
            WriteLog(LogMessageType.Info, "Start");
        }

        public void OnStop()
        {
            WriteLog(LogMessageType.Info, "Stop");
        }

        public void UpdateStatus(string status)
        {
            lock (_sync)
            {
                Status = status;
                StatusUpdated?.Invoke(status);
            }
        }

        private void WriteLog(LogMessageType type, string message)
        {
            var msg = new LogMessage(type, message);

            switch (type)
            {
                case LogMessageType.Custom:
                case LogMessageType.Info:
                case LogMessageType.Trading:
                    _logger.Info(msg.ToString());
                    break;
                case LogMessageType.Error:
                    _logger.Error(msg.ToString());
                    break;
            }

            lock (_internalSync)
            {
                if (_logMessages.Count >= _keepInmemory)
                    _logMessages.RemoveFisrt();

                _logMessages.Add(msg);
            }
        }

        private ILogger GetLogger(string botId)
        {
            var loggerName = PathEscaper.Escape(botId);
            var logTarget = $"all-{botId}";
            var errTarget = $"error-{botId}";

            var logFile = new FileTarget(logTarget)
            {
                FileName = Layout.FromString($"{ServerModel.Environment.BotLogFolder}/{loggerName}/${{shortdate}}-log.txt"),
                Layout = Layout.FromString("${longdate} | ${logger} | ${message}")
            };

            var errorFile = new FileTarget(errTarget)
            {
                FileName = Layout.FromString($"{ServerModel.Environment.BotLogFolder}/{loggerName}/${{shortdate}}-error.txt"),
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
    }
}
