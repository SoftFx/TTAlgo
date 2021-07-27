using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class RemoteTradeBot : ITradeBot
    {
        private RemoteAlgoAgent _agent;
        private int _statusSubscriptionCnt;
        private int _logsSubscriptionCnt;

        private bool _subscribeStatusEnable = false;
        private bool _subscribeLogsEnable = false;


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
            Journal = new BotJournal(info.InstanceId);
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

        public void UpdateStatus(string status)
        {
            if (status != Status)
            {
                Status = status;
                StatusChanged?.Invoke(this);
            }
        }

        public void UpdateLogs(List<LogRecordInfo> logs)
        {
            if (State == PluginModelInfo.Types.PluginState.Running)
                Journal.Add(logs.Select(Convert).ToList());
        }

        public void SubscribeToStatus()
        {
            _statusSubscriptionCnt++;
            ManageSubscribeToStatus();
        }

        public void UnsubscribeFromStatus()
        {
            _statusSubscriptionCnt--;
            ManageSubscribeToStatus();
        }

        public void SubscribeToLogs()
        {
            _logsSubscriptionCnt++;
            ManageSubscribeToLogs();
        }

        public void UnsubscribeFromLogs()
        {
            _logsSubscriptionCnt--;
            ManageSubscribeToLogs();
        }

        private async void ManageSubscribeToStatus()
        {
            if (_statusSubscriptionCnt < 0)
                _statusSubscriptionCnt = 0;

            if (_statusSubscriptionCnt > 0 && !_subscribeStatusEnable)
            {
                _subscribeStatusEnable = true;

                await _agent.SubscribeToPluginStatus(InstanceId);
            }
            else
            if (_statusSubscriptionCnt == 0 && _subscribeStatusEnable)
            {
                _subscribeStatusEnable = false;

                await _agent.UnsubscribeToPluginStatus(InstanceId);
            }
        }

        private async void ManageSubscribeToLogs()
        {
            if (_logsSubscriptionCnt < 0)
                _logsSubscriptionCnt = 0;

            if (_logsSubscriptionCnt > 0 && !_subscribeLogsEnable)
            {
                _subscribeLogsEnable = true;

                await _agent.SubscribeToPluginLogs(InstanceId);
            }
            else
            if (_logsSubscriptionCnt == 0 && _subscribeLogsEnable)
            {
                _subscribeLogsEnable = false;

                await _agent.UnsubscribeToPluginLogs(InstanceId);
            }
        }

        private void ResetJournal()
        {
            Journal.Clear();
        }

        private BotMessage Convert(LogRecordInfo record)
        {
            return new BotMessage(record.TimeUtc, InstanceId, record.Message, Convert(record.Severity));
        }

        private static JournalMessageType Convert(PluginLogRecord.Types.LogSeverity severity)
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
