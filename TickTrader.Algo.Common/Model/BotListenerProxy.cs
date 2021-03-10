using System;
using System.Threading;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Common.Model
{
    public interface IBotWriter
    {
        void LogMesssage(Domain.PluginLogRecord record);

        void UpdateStatus(string status);

        void Trace(string status);

        void Close();
    }


    public class BotListenerProxy : CrossDomainObject
    {
        private ExecutorModel _executor;
        private Action _onStopped;
        private IBotWriter _writer;
        private string _currentStatus;
        private Timer _timer;


        public BotListenerProxy(ExecutorModel executor, Action onStopped, IBotWriter writer)
        {
            _executor = executor;
            _onStopped = onStopped;
            _writer = writer;

            executor.Config.IsLoggingEnabled = true;
            executor.Stopped += Executor_Stopped;
            executor.LogUpdated += Executor_LogUpdated;
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
                _writer.Close();
            }
        }


        protected override void Dispose(bool disposing)
        {
            _timer?.Dispose();

            if (disposing)
            {
                _executor.Stopped -= Executor_Stopped;
                _executor.LogUpdated -= Executor_LogUpdated;
            }

            base.Dispose(disposing);
        }


        private void Executor_LogUpdated(Domain.PluginLogRecord record)
        {
            if (record.Severity != Domain.PluginLogRecord.Types.LogSeverity.CustomStatus)
                _writer.LogMesssage(record);
            else
            {
                _currentStatus = record.Message;
                _writer.UpdateStatus(record.Message);
            }
        }

        private void Executor_Stopped(ExecutorModel executor)
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
