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

namespace TickTrader.BotTerminal
{
    internal class TradeBotModel : PluginModel, IBotWriter
    {
        private BotListenerProxy _botListener;


        public BotModelStates State { get; private set; }
        public string CustomStatus { get; private set; }
        public bool StateViewOpened { get; set; }
        public SettingsStorage<WindowStorageModel> StateViewSettings { get; private set; }


        public event System.Action<TradeBotModel> CustomStatusChanged = delegate { };
        public event System.Action<TradeBotModel> StateChanged = delegate { };
        public event System.Action<TradeBotModel> Removed = delegate { };
        public event System.Action<TradeBotModel> ConfigurationChanged = delegate { };


        public TradeBotModel(PluginSetupViewModel pSetup, IAlgoPluginHost host, WindowStorageModel stateSettings)
            : base(pSetup, host)
        {
            host.Journal.RegisterBotLog(InstanceId);
            host.Connected += Host_Connected;
            StateViewSettings = new SettingsStorage<WindowStorageModel>(stateSettings);
        }

        internal void Abort()
        {
            if (State == BotModelStates.Stopping)
                AbortExecutor();
        }

        public void Start()
        {
            if (State != BotModelStates.Stopped)
                return;
            Host.Lock();
            if (StartExcecutor())
            {
                _botListener?.Start();
                ChangeState(BotModelStates.Running);
            }
        }

        public async Task Stop()
        {
            if (State != BotModelStates.Running)
                return;
            ChangeState(BotModelStates.Stopping);
            await StopExecutor();
            _botListener?.Stop();
            ChangeState(BotModelStates.Stopped);
            Host.Unlock();
        }

        public void Remove()
        {
            Host.Journal.UnregisterBotLog(InstanceId);
            Removed(this);
        }


        protected override PluginExecutor CreateExecutor()
        {
            var executor = base.CreateExecutor();
            executor.TradeExecutor = Host.GetTradeApi();
            executor.WorkingFolder = Path.Combine(EnvService.Instance.AlgoWorkingFolder, PathHelper.GetSafeFileName(InstanceId));
            executor.BotWorkingFolder = executor.WorkingFolder;
            executor.TradeHistoryProvider = Host.GetTradeHistoryApi();
            EnvService.Instance.EnsureFolder(executor.WorkingFolder);

            _botListener = new BotListenerProxy(executor, StopInternal, this);
            return executor;
        }

        new internal void Configurate(PluginSetup setup, PluginPermissions permissions, bool isolated)
        {
            if (State != BotModelStates.Stopped)
                return;

            base.Configurate(setup, permissions, isolated);

            ConfigurationChanged(this);
        }

        private void ChangeState(BotModelStates newState)
        {
            State = newState;
            StateChanged(this);
        }

        private void TradeBotModel2_NewRecords(BotLogRecord[] records)
        {
            List<BotMessage> messages = new List<BotMessage>(records.Length);
            string status = null;

            foreach (var rec in records)
            {
                if (rec.Severity != LogSeverities.CustomStatus)
                    messages.Add(Convert(rec));
                else
                    status = rec.Message;
            }

            if (messages.Count > 0)
                Host.Journal.Add(messages);

            if (status != null)
            {
                Execute.OnUIThread(() =>
                {
                    CustomStatus = status;
                    CustomStatusChanged?.Invoke(this);
                });
            }
        }

        private void Host_Connected()
        {
            if (this.State == BotModelStates.Running)
                HandleReconnect();
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
                default: return JournalMessageType.Info;
            }
        }

        private void StopInternal()
        {
            Execute.OnUIThread(() =>
            {
                if (State == BotModelStates.Running)
                {
                    ChangeState(BotModelStates.Stopped);
                    Host.Unlock();
                }
            });
        }


        #region IBotWriter implementation

        void IBotWriter.LogMesssages(IEnumerable<BotLogRecord> records)
        {
            List<BotMessage> messages = new List<BotMessage>();

            foreach (var rec in records)
            {
                if (rec.Severity != LogSeverities.CustomStatus)
                    messages.Add(Convert(rec));
            }

            if (messages.Count > 0)
                Host.Journal.Add(messages);
        }

        void IBotWriter.UpdateStatus(string status)
        {
            Execute.OnUIThread(() =>
            {
                CustomStatus = status;
                CustomStatusChanged?.Invoke(this);
            });
        }

        void IBotWriter.LogStatus(string status)
        {
            Host.Journal.LogStatus(InstanceId, status);
        }

        #endregion IBotWriter implementation
    }
}
