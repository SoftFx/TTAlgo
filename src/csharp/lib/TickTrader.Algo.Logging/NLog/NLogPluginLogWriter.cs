using NLog;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Logging
{
    public sealed class NLogPluginLogWriter : IPluginLogWriter
    {
        private LogFactory _logFactory;
        private ILogger _logger;
        private string _lastStatus;
        private bool _isStopped;


        private NLogPluginLogWriter() { }


        public static IPluginLogWriter Create() => new NLogPluginLogWriter();


        public void Start(string logDir)
        {
            var logConfig = NLogHelper.CreatePluginConfig(logDir);

            _logFactory = new LogFactory(logConfig);
            _logger = _logFactory.GetLogger("plugin");
            _logFactory.AutoShutdown = false;
        }

        public void Stop()
        {
            _isStopped = true;
            _logFactory.Shutdown();
        }

        public void OnBotStarted()
        {
            _isStopped = false;
            _logger.Trace("Bot started");
            Task.Delay(1000).ContinueWith(_ => LogStatusLoop());
        }

        public void OnBotStopped()
        {
            _isStopped = true;
            LogStatusSnapshot();
            _logger.Trace("Bot stopped");

        }

        public void OnLogRecord(PluginLogRecord record)
        {
            var logEvent = CreateLogEvent(record);
            if (logEvent != null)
                _logger.Log(logEvent);
        }

        public void OnStatusUpdate(PluginStatusUpdate update)
        {
            _lastStatus = update.Message;
        }


        private LogEventInfo CreateLogEvent(PluginLogRecord record)
        {
            var logLevel = LogLevel.Info;
            switch (record.Severity)
            {
                case PluginLogRecord.Types.LogSeverity.Alert:
                    logLevel = LogLevel.Warn;
                    break;
                case PluginLogRecord.Types.LogSeverity.Error:
                    logLevel = LogLevel.Error;
                    break;
            }

            var logEvent = LogEventInfo.Create(logLevel, "plugin", RenderMessage(record));
            logEvent.TimeStamp = record.TimeUtc.ToDateTime();
            return logEvent;
        }

        private string RenderMessage(PluginLogRecord record)
        {
            return string.IsNullOrEmpty(record.Details)
                ? $"{record.Severity} | {record.Message}"
                : $"{record.Severity} | {record.Message} {Environment.NewLine} {record.Details}";
        }

        private void LogStatusLoop()
        {
            if (_isStopped)
                return;

            LogStatusSnapshot();

            if (!_isStopped)
                Task.Delay(PluginLogWriter.StatusWriteTimeout).ContinueWith(_ => LogStatusLoop());
        }

        private void LogStatusSnapshot()
        {
            var status = _lastStatus;
            if (string.IsNullOrWhiteSpace(_lastStatus))
                status = "<empty>";

            _logger.Trace(string.Join(Environment.NewLine, "Status snapshot", status, ""));
        }
    }
}
