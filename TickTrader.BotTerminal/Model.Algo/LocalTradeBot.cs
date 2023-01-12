using System;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Server;

namespace TickTrader.BotTerminal
{
    internal sealed class LocalTradeBot : ITradeBot
    {
        private readonly LocalAlgoAgent2 _agent;

        private int _statusSubscriptionCnt;
        private int _logsSubscriptionCnt;
        private PluginListenerProxy _listener;


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


        public LocalTradeBot(PluginModelInfo info, LocalAlgoAgent2 agent)
        {
            Info = info;

            _agent = agent;

            _statusSubscriptionCnt = 0;
            _logsSubscriptionCnt = 0;
            Journal = new BotJournal(Info.InstanceId);

            Journal.Clear();
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
            ManagePluginListener();
        }

        public void UnsubscribeFromStatus()
        {
            _statusSubscriptionCnt--;
            ManagePluginListener();
        }

        public void SubscribeToLogs()
        {
            _logsSubscriptionCnt++;
            ManagePluginListener();
        }

        public void UnsubscribeFromLogs()
        {
            _logsSubscriptionCnt--;
            ManagePluginListener();
        }


        private void ManagePluginListener()
        {
            var needListener = _statusSubscriptionCnt > 0 || _logsSubscriptionCnt > 0;

            if (needListener && _listener == null)
            {
                _listener = _agent.GetPluginListener(InstanceId);
                _ = _listener.Init();
                _ = PullListenerUpdates();
            }
            else if (!needListener && _listener != null)
            {
                _listener.Dispose();
                _listener = null;
            }
        }

        private async Task PullListenerUpdates()
        {
            var listener = _listener;
            while (_listener == listener)
            {
                UpdateStatus(_listener.GetLastStatus());
                UpdateLogs(_listener.GetLastLogs());

                await Task.Delay(200);
            }
        }

        private void UpdateStatus(PluginStatusUpdate update)
        {
            if (update != null && update.Message != Status)
            {
                Status = update.Message;
                StatusChanged?.Invoke(this);
            }
        }

        private void UpdateLogs(PluginLogRecord[] logs)
        {
            Journal.Add(logs.Where(l => ApplyNewRecordsFilter(new TimeKey(l.TimeUtc))).Select(Convert).ToList());
        }

        private BotMessage Convert(PluginLogRecord record)
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

        private bool ApplyNewRecordsFilter(TimeKey timeKey)
        {
            var isTailMessage = true;
            var isClearedMessage = timeKey.CompareTo(Journal.TimeLastClearedMessage) != 1;

            if (Journal.Records.Count > 0)
                isTailMessage = timeKey.CompareTo(Journal.Records.Last().TimeKey) == 1;

            return isTailMessage && !isClearedMessage;
        }
    }
}