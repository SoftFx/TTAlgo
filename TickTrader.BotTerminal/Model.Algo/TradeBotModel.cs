using Caliburn.Micro;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Common.Model.Setup;
using System.IO;
using System.Threading.Tasks.Dataflow;
using System.Linq;
using System.Collections.Generic;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    internal interface ITradeBot
    {
        bool IsRemote { get; }

        string InstanceId { get; }

        PluginConfig Config { get; }

        PluginStates State { get; }

        string FaultMessage { get; }

        PluginDescriptor Descriptor { get; }

        string Status { get; }

        BotJournal Journal { get; }

        AccountKey Account { get; }


        event Action<ITradeBot> ConfigurationChanged;
        event Action<ITradeBot> StateChanged;
        event Action<ITradeBot> StatusChanged;


        void SubscribeToStatus();

        void UnsubscribeFromStatus();

        void SubscribeToLogs();

        void UnsubscribeFromLogs();
    }


    internal class TradeBotModel : PluginModel, IBotWriter, ITradeBot
    {
        private BotListenerProxy _botListener;


        public bool IsRemote => false;
        public string Status { get; private set; }
        public BotJournal Journal { get; }
        public AccountKey Account { get; }


        public event Action<ITradeBot> StatusChanged = delegate { };
        public event Action<ITradeBot> StateChanged = delegate { };
        public event Action<ITradeBot> ConfigurationChanged = delegate { };


        public TradeBotModel(PluginConfig config, LocalAlgoAgent agent, IAlgoPluginHost host, IAlgoSetupContext setupContext, AccountKey account)
            : base(config, agent, host, setupContext)
        {
            Account = account;
            Journal = new BotJournal(InstanceId);
            host.Connected += Host_Connected;
            host.Disconnected += Host_Disconnected;
        }

        internal void Abort()
        {
            if (State == PluginStates.Stopping)
                AbortExecutor();
        }

        public void Start()
        {
            if (State != PluginStates.Stopped)
                return;
            Host.Lock();
            if (StartExcecutor())
            {
                _botListener?.Start();
                ChangeState(PluginStates.Running);
            }
        }

        public async Task Stop()
        {
            if (State != PluginStates.Running)
                return;
            await StopExecutor();
            _botListener?.Stop();
            ChangeState(PluginStates.Stopped);
            Host.Unlock();
            if (PackageRef?.IsObsolete ?? false)
            {
                UpdateRefs();
            }
        }


        protected override PluginExecutor CreateExecutor()
        {
            var executor = base.CreateExecutor();
            executor.TradeExecutor = Host.GetTradeApi();
            executor.WorkingFolder = Path.Combine(EnvService.Instance.AlgoWorkingFolder, PathHelper.GetSafeFileName(InstanceId));
            executor.BotWorkingFolder = executor.WorkingFolder;
            executor.TradeHistoryProvider = Host.GetTradeHistoryApi();
            EnvService.Instance.EnsureFolder(executor.WorkingFolder);

            _botListener = new BotListenerProxy(executor, OnBotExited, this);
            return executor;
        }

        internal override void Configurate(PluginConfig config)
        {
            if (State != PluginStates.Stopped)
                return;

            base.Configurate(config);

            ConfigurationChanged(this);
        }

        protected override void ChangeState(PluginStates state, string faultMessage = null)
        {
            base.ChangeState(state, faultMessage);
            StateChanged(this);
        }

        protected override void LockResources()
        {
            base.LockResources();
            Host.Lock();
        }

        protected override void UnlockResources()
        {
            base.UnlockResources();
            Host.Unlock();
        }

        private void Host_Connected()
        {
            if (State == PluginStates.Reconnecting)
            {
                HandleReconnect();
                ChangeState(PluginStates.Running);
            }
        }

        private void Host_Disconnected()
        {
            if (State == PluginStates.Running)
            {
                HandleDisconnect();
                ChangeState(PluginStates.Reconnecting);
            }
        }

        private BotMessage Convert(BotLogRecord record)
        {
            return new BotMessage(record.Time, InstanceId, record.Message, Convert(record.Severity)) { Details = record.Details };
        }

        private JournalMessageType Convert(LogSeverities severity)
        {
            switch (severity)
            {
                case LogSeverities.Info: return JournalMessageType.Info;
                case LogSeverities.Error: return JournalMessageType.Error;
                case LogSeverities.Custom: return JournalMessageType.Custom;
                case LogSeverities.Trade: return JournalMessageType.Trading;
                case LogSeverities.TradeSuccess: return JournalMessageType.TradingSuccess;
                case LogSeverities.TradeFail: return JournalMessageType.TradingFail;
                default: return JournalMessageType.Info;
            }
        }

        private void OnBotExited()
        {
            Execute.OnUIThread(() =>
            {
                if (State == PluginStates.Running)
                {
                    ChangeState(PluginStates.Stopped);
                    UnlockResources();
                }
            });
        }


        #region ITradeBot stubs

        public void SubscribeToStatus() { }

        public void UnsubscribeFromStatus() { }

        public void SubscribeToLogs() { }

        public void UnsubscribeFromLogs() { }

        #endregion


        #region IBotWriter implementation

        void IBotWriter.LogMesssages(IEnumerable<BotLogRecord> records)
        {
            var messages = new List<BotMessage>();

            foreach (var rec in records)
            {
                if (rec.Severity != LogSeverities.CustomStatus)
                    messages.Add(Convert(rec));
            }

            if (messages.Count > 0)
                Journal.Add(messages);
        }

        void IBotWriter.UpdateStatus(string status)
        {
            Execute.OnUIThread(() =>
            {
                Status = status;
                StatusChanged?.Invoke(this);
            });
        }

        void IBotWriter.Trace(string status)
        {
            Journal.LogStatus(status);
        }

        #endregion IBotWriter implementation
    }
}
