using NLog;
using System;
using System.Linq;
using System.Threading;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    internal class RemoteTradeBot : ITradeBot, IDisposable
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private const int StatusUpdateTimeout = 1000;
        private const int LogsUpdateTimeout = 1000;
        private const int AlertsUpdateTimeout = 1000;

        private RemoteAlgoAgent _agent;
        private int _statusSubscriptionCnt;
        private int _logsSubscriptionCnt;
        private int _alertsSubscriptionCnt;
        private Timer _statusTimer;
        private Timer _logsTimer;
        private Timer _alertTimer;
        private DateTime _lastLogTimeUtc;
        private DateTime _lastAlertTimeUtc;


        public BotModelInfo Info { get; private set; }


        public bool IsRemote => true;

        public string InstanceId => Info.InstanceId;

        public PluginConfig Config => Info.Config;

        public PluginStates State => Info.State;

        public string FaultMessage => Info.FaultMessage;

        public PluginDescriptor Descriptor => Info.Descriptor;

        public string Status { get; private set; }

        public BotJournal Journal { get; }

        public AccountKey Account => Info.Account;


        public event Action<ITradeBot> Updated;
        public event Action<ITradeBot> StateChanged;
        public event Action<ITradeBot> StatusChanged;


        public RemoteTradeBot(BotModelInfo info, RemoteAlgoAgent agent)
        {
            Info = info;
            _agent = agent;

            _statusSubscriptionCnt = 0;
            _logsSubscriptionCnt = 0;
            Journal = new BotJournal(info.InstanceId, false);
            ResetJournal();
        }


        public void Update(BotModelInfo info)
        {
            Info = info;
            Updated?.Invoke(this);
        }

        public void UpdateState(BotModelInfo info)
        {
            Info.State = info.State;
            Info.FaultMessage = info.FaultMessage;
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

        public void SubscribeToAlerts()
        {
            _alertsSubscriptionCnt++;
            ManageAlertTimer();
        }

        public void UnsubscribeFromAlerts()
        {
            _alertsSubscriptionCnt--;
            ManageAlertTimer();
        }

        public void Dispose()
        {
            _statusTimer.Dispose();
            _logsTimer.Dispose();
            _alertTimer.Dispose();
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

        private void ManageAlertTimer()
        {
            if (_alertsSubscriptionCnt < 0)
                _alertsSubscriptionCnt = 0;

            if (_alertsSubscriptionCnt > 0 && _alertTimer == null)
            {
                _alertTimer = new Timer(UpdateAlert, null, AlertsUpdateTimeout, -1);
            }
            else if (_alertsSubscriptionCnt == 0 && _alertTimer != null)
            {
                _alertTimer.Dispose();
                _alertTimer = null;
            }
        }

        private void ResetJournal()
        {
            _lastLogTimeUtc = new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _lastAlertTimeUtc = new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc);
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
                    _lastLogTimeUtc = logs.Max(l => l.TimeUtc).Timestamp;

                    Journal.Add(logs.Select(Convert).ToList());
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to get bot logs {InstanceId} at {_agent.Name}");
            }

            _logsTimer?.Change(LogsUpdateTimeout, -1);
        }

        private async void UpdateAlert(object state)
        {
            _alertTimer?.Change(-1, -1);
            try
            {
                var alerts = await _agent.GetBotLogs(InstanceId, _lastAlertTimeUtc, getAlert: true);
                if (alerts.Length > 0)
                {
                    _lastAlertTimeUtc = alerts.Max(l => l.TimeUtc).Timestamp;
                    _agent.AlertModel.AddAlerts(alerts.Select(Convert).ToList());
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to get bot alerts {InstanceId} at {_agent.Name}");
            }

            _alertTimer?.Change(AlertsUpdateTimeout, -1);
        }

        private BotMessage Convert(LogRecordInfo record)
        {
            return new BotMessage(record.TimeUtc.ToLocalTime(), InstanceId, record.Message, Convert(record.Severity));
        }

        private JournalMessageType Convert(LogSeverity severity)
        {
            switch (severity)
            {
                case LogSeverity.Info: return JournalMessageType.Info;
                case LogSeverity.Error: return JournalMessageType.Error;
                case LogSeverity.Custom: return JournalMessageType.Custom;
                case LogSeverity.Trade: return JournalMessageType.Trading;
                case LogSeverity.TradeSuccess: return JournalMessageType.TradingSuccess;
                case LogSeverity.TradeFail: return JournalMessageType.TradingFail;
                case LogSeverity.Alert: return JournalMessageType.Alert;
                default: return JournalMessageType.Info;
            }
        }
    }
}
