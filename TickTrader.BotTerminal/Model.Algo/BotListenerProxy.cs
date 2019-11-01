using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Threading;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotTerminal
{
    public interface IBotWriter
    {
        void LogMesssage(PluginLogRecord records);

        void UpdateStatus(string status);

        void Trace(string status);
    }

    public class BotListenerProxy
    {
        private PluginExecutor _executor;
        private Action _onStopped;
        private IBotWriter _writer;
        //private object _sync = new object();
        private string _currentStatus;
        private DispatcherTimer _timer;

        public BotListenerProxy(PluginExecutor executor, Action onStopped, IBotWriter writer)
        {
            _executor = executor;
            _onStopped = onStopped;
            _writer = writer;

            executor.Config.IsLoggingEnabled = true;
            executor.Stopped += Executor_Stopped; //IsRunning .IsRunningChanged += Executor_IsRunningChanged;
            executor.LogUpdated += Executor_LogUpdated; //NewRecords += ListenerProxy_NewRecords;
        }

        public void Start()
        {
            _writer.Trace("Bot started");
            _timer = new DispatcherTimer(DispatcherPriority.Background);  //new Timer(LogStatus, null, , TimeSpan.FromMinutes(1));
            _timer.Interval = TimeSpan.FromMinutes(1);
            _timer.Tick += (s, a) => LogStatus();
            _timer.Start();
        }

        public void Stop()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
                LogStatus();
                _writer.Trace("Bot stopped");
            }
        }

        protected void Dispose(bool disposing)
        {
            _timer?.Stop();

            if (disposing)
            {
                //_executor.IsRunningChanged -= Executor_IsRunningChanged;
                _executor.Stopped -= Executor_Stopped;
                _executor.LogUpdated -= Executor_LogUpdated;
            }
        }

        private void Executor_LogUpdated(PluginLogRecord record)
        {
            if (record.Severity != LogSeverities.CustomStatus)
                _writer.LogMesssage(record);
            else
            {
                _currentStatus = record.Message;
                _writer.UpdateStatus(record.Message);
            }
        }

        private void Executor_Stopped(PluginExecutor executor)
        {
            _onStopped();
        }

        private void LogStatus()
        {
            if (!string.IsNullOrEmpty(_currentStatus))
            {
                _writer.Trace(string.Join(Environment.NewLine, "Status snapshot", _currentStatus, ""));
            }
        }
    }
}
