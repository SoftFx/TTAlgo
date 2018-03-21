using System;
using System.Collections.Generic;
using System.Threading;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Common.Model
{
    public interface IBotWriter
    {
        void LogMesssages(IEnumerable<BotLogRecord> records);

        void UpdateStatus(string status);

        void Trace(string status);
    }


    public class BotListenerProxy : CrossDomainObject
    {
        private PluginExecutor _executor;
        private Action _onStopped;
        private IBotWriter _writer;
        private object _sync = new object();
        private string _currentStatus;
        private Timer _timer;


        public BotListenerProxy(PluginExecutor executor, Action onStopped, IBotWriter writer)
        {
            _executor = executor;
            _onStopped = onStopped;
            _writer = writer;

            executor.IsRunningChanged += Executor_IsRunningChanged;
            executor.InitLogging().NewRecords += ListenerProxy_NewRecords;
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

            _executor.IsRunningChanged -= Executor_IsRunningChanged;
            _executor.InitLogging().NewRecords -= ListenerProxy_NewRecords;

            base.Dispose(disposing);
        }


        private void ListenerProxy_NewRecords(BotLogRecord[] records)
        {
            lock (_sync)
            {
                var messages = new List<BotLogRecord>(records.Length);
                string status = null;

                foreach (var record in records)
                {
                    if (record.Severity != LogSeverities.CustomStatus)
                        messages.Add(record);
                    else
                        status = record.Message;
                }

                if (status != null)
                {
                    _currentStatus = status;
                    _writer.UpdateStatus(status);
                }

                if (messages.Count > 0)
                {
                    _writer.LogMesssages(messages);
                }
            }
        }

        private void Executor_IsRunningChanged(PluginExecutor exec)
        {
            if (!exec.IsRunning)
                _onStopped();
        }

        private void LogStatus(object state)
        {
            lock (_sync)
            {
                if (!string.IsNullOrWhiteSpace(_currentStatus))
                {
                    _writer.Trace(string.Join(Environment.NewLine, "Status snapshot", _currentStatus, ""));
                }
            }
        }
    }
}
