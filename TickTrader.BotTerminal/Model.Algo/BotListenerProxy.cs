using System;
using System.Windows.Threading;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Server;

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
        private readonly Action _onStopped;
        private readonly IBotWriter _writer;
        private readonly IDisposable _logSub, _statusSub, _stoppedSub;

        private string _currentStatus;
        private DispatcherTimer _timer;

        public BotListenerProxy(ExecutorModel executor, Action onStopped, IBotWriter writer)
        {
            _onStopped = onStopped;
            _writer = writer;

            _logSub = executor.LogUpdated.Subscribe(Executor_LogUpdated);
            _statusSub = executor.StatusUpdated.Subscribe(Executor_StatusUpdated);
            _stoppedSub = executor.Stopped.Subscribe(Executor_Stopped);
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

        public void Dispose()
        {
            _timer?.Stop();

            _logSub.Dispose();
            _statusSub.Dispose();
            _stoppedSub.Dispose();
        }

        private void Executor_LogUpdated(PluginLogRecord record)
        {
            _writer.LogMesssage(record);
        }

        private void Executor_StatusUpdated(PluginStatusUpdate update)
        {
            _currentStatus = update.Message;
            _writer.UpdateStatus(update.Message);
        }

        private void Executor_Stopped(bool val)
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
