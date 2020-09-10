using System;
using System.Threading;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Common.Model
{
    public interface IBotWriter
    {
        void LogMesssage(Domain.UnitLogRecord record);

        void UpdateStatus(string status);

        void Trace(string status);
    }


    public class BotListenerProxy : CrossDomainObject
    {
        private RuntimeModel _runtime;
        private Action _onStopped;
        private IBotWriter _writer;
        private string _currentStatus;
        private Timer _timer;


        public BotListenerProxy(RuntimeModel runtime, Action onStopped, IBotWriter writer)
        {
            _runtime = runtime;
            _onStopped = onStopped;
            _writer = writer;

            runtime.Config.IsLoggingEnabled = true;
            runtime.Stopped += Executor_Stopped;
            runtime.LogUpdated += Executor_LogUpdated;
        }


        public void Start()
        {
            _writer.Trace("Bot started");
            _timer = new Timer(LogStatus, null, TimeSpan.FromMilliseconds(1000), TimeSpan.FromMinutes(1));
        }

        public void Stop()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
                LogStatus(null);
                _writer.Trace("Bot stopped");
            }
        }


        protected override void Dispose(bool disposing)
        {
            _timer?.Dispose();

            if (disposing)
            {
                _runtime.Stopped -= Executor_Stopped;
                _runtime.LogUpdated -= Executor_LogUpdated;
            }

            base.Dispose(disposing);
        }


        private void Executor_LogUpdated(Domain.UnitLogRecord record)
        {
            if (record.Severity != Domain.UnitLogRecord.Types.LogSeverity.CustomStatus)
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

        private void LogStatus(object state)
        {
            if (!string.IsNullOrWhiteSpace(_currentStatus))
            {
                _writer.Trace(string.Join(Environment.NewLine, "Status snapshot", _currentStatus, ""));
            }
        }
    }
}
