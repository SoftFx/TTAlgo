using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Threading;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    public interface IBotWriter
    {
        void LogMesssage(UnitLogRecord records);

        void UpdateStatus(string status);

        void Trace(string status);
    }

    public class BotListenerProxy
    {
        private RuntimeModel _runtime;
        private Action _onStopped;
        private IBotWriter _writer;
        //private object _sync = new object();
        private string _currentStatus;
        private DispatcherTimer _timer;

        public BotListenerProxy(RuntimeModel runtime, Action onStopped, IBotWriter writer)
        {
            _runtime = runtime;
            _onStopped = onStopped;
            _writer = writer;

            runtime.Stopped += Executor_Stopped; //IsRunning .IsRunningChanged += Executor_IsRunningChanged;
            runtime.LogUpdated += Executor_LogUpdated; //NewRecords += ListenerProxy_NewRecords;
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
                _runtime.Stopped -= Executor_Stopped;
                _runtime.LogUpdated -= Executor_LogUpdated;
            }
        }

        private void Executor_LogUpdated(UnitLogRecord record)
        {
            if (record.Severity != UnitLogRecord.Types.LogSeverity.CustomStatus)
                _writer.LogMesssage(record);
            else
            {
                _currentStatus = record.Message;
                _writer.UpdateStatus(record.Message);
            }
        }

        private void Executor_Stopped(RuntimeModel runtime)
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
