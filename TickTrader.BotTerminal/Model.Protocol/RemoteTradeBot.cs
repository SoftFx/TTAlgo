using Google.Protobuf.WellKnownTypes;
using NLog;
using System;
using System.Linq;
using System.Threading;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Domain;
using TickTrader.Algo.ServerControl;

namespace TickTrader.BotTerminal
{
    internal class RemoteTradeBot : ITradeBot, IDisposable
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private const int StatusUpdateTimeout = 1000;
        private const int LogsUpdateTimeout = 1000;


        private RemoteAlgoAgent _agent;
        private int _statusSubscriptionCnt;
        private int _logsSubscriptionCnt;
        private Timer _statusTimer;
        private Timer _logsTimer;
        private Timestamp _lastLogTimeUtc;


        public PluginModelInfo Info { get; private set; }


        public bool IsRemote => true;

        public string InstanceId => Info.InstanceId;

        public PluginConfig Config => Info.Config;

        public PluginModelInfo.Types.PluginState State => Info.State;

        public string FaultMessage => Info.FaultMessage;

        public PluginDescriptor Descriptor => Info.Descriptor_;

        public string Status { get; private set; }

        public BotJournal Journal { get; }

        public string AccountId => Info.AccountId;


        public event Action<ITradeBot> Updated;
        public event Action<ITradeBot> StateChanged;
        public event Action<ITradeBot> StatusChanged;


        public RemoteTradeBot(PluginModelInfo info, RemoteAlgoAgent agent)
        {
            Info = info;
            _agent = agent;

            _statusSubscriptionCnt = 0;
            _logsSubscriptionCnt = 0;
            Journal = new BotJournal(info.InstanceId, false);
            ResetJournal();
        }


        public void Update(PluginModelInfo info)
        {
            Info = info;
            Updated?.Invoke(this);
        }

        public void UpdateState(PluginStateUpdate update)
        {
            Info.State = update.State;
            Info.FaultMessage = update.FaultMessage;
            StateChanged?.Invoke(this);
        }

        public void SubscribeToStatus()
        {
            _statusSubscriptionCnt++;
            ManageStatusTimer();
        }

        public void UnsubscribeFromStatus()
        {
            _statusSubscriptionCnt--;
            ManageStatusTimer();
        }

        public void SubscribeToLogs()
        {
            _logsSubscriptionCnt++;
            ManageLogsTimer();
        }

        public void UnsubscribeFromLogs()
        {
            _logsSubscriptionCnt--;
            ManageLogsTimer();
        }

        public void Dispose()
        {
            _statusTimer.Dispose();
            _logsTimer.Dispose();
        }


        private void ManageStatusTimer()
        {
            if (_statusSubscriptionCnt < 0)
                _statusSubscriptionCnt = 0;

            if (_statusSubscriptionCnt > 0 && _statusTimer == null)
            {
                _statusTimer = new Timer(UpdateStatus, null, StatusUpdateTimeout, -1);
            }
            else if (_statusSubscriptionCnt == 0 && _statusTimer != null)
            {
                _statusTimer.Dispose();
                _statusTimer = null;
            }
        }

        private void ManageLogsTimer()
        {
            if (_logsSubscriptionCnt < 0)
                _logsSubscriptionCnt = 0;

            if (_logsSubscriptionCnt > 0 && _logsTimer == null)
            {
                _logsTimer = new Timer(UpdateLogs, null, LogsUpdateTimeout, -1);
            }
            else if (_logsSubscriptionCnt == 0 && _logsTimer != null)
            {
                _logsTimer.Dispose();
                _logsTimer = null;
            }
        }

        private void ResetJournal()
        {
            _lastLogTimeUtc = new Timestamp(); // 1970-01-01
            Journal.Clear();
        }

        private async void UpdateStatus(object state)
        {
            _statusTimer?.Change(-1, -1);
            try
            {
                var status = await _agent.GetBotStatus(InstanceId);
                if (status != Status)
                {
                    Status = status;
                    StatusChanged?.Invoke(this);
                }
            }
            catch(Algo.ServerControl.AlgoException baex)
            {
                _logger.Error($"Failed to get bot status {InstanceId} at {_agent.Name}: {baex.Message}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to get bot status {InstanceId} at {_agent.Name}");
            }
            _statusTimer?.Change(StatusUpdateTimeout, -1);
        }

        private async void UpdateLogs(object state)
        {
            _logsTimer?.Change(-1, -1);
            try
            {
                var logs = await _agent.GetBotLogs(InstanceId, _lastLogTimeUtc);
                if (logs.Length > 0)
                {
                    _lastLogTimeUtc = logs.Max(l => l.TimeUtc);

                    Journal.Add(logs.Select(this.Convert).ToList());
                }
            }
            catch (Algo.ServerControl.AlgoException baex)
            {
                _logger.Error($"Failed to get bot logs {InstanceId} at {_agent.Name}: {baex.Message}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to get bot logs {InstanceId} at {_agent.Name}");
            }

            _logsTimer?.Change(LogsUpdateTimeout, -1);
        }

        private BotMessage Convert(LogRecordInfo record)
        {
            return new BotMessage(record.TimeUtc, InstanceId, record.Message, Convert(record.Severity));
        }

        private JournalMessageType Convert(PluginLogRecord.Types.LogSeverity severity)
        {
            switch (severity)
            {
                case PluginLogRecord.Types.LogSeverity.Info: return JournalMessageType.Info;
                case PluginLogRecord.Types.LogSeverity.Error: return JournalMessageType.Error;
                case PluginLogRecord.Types.LogSeverity.Custom: return JournalMessageType.Custom;
                case PluginLogRecord.Types.LogSeverity.Trade: return JournalMessageType.Trading;
                case PluginLogRecord.Types.LogSeverity.TradeSuccess: return JournalMessageType.TradingSuccess;
                case PluginLogRecord.Types.LogSeverity.TradeFail: return JournalMessageType.TradingFail;
                case PluginLogRecord.Types.LogSeverity.Alert: return JournalMessageType.Alert;
                default: return JournalMessageType.Info;
            }
        }
    }
}
