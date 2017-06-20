using Caliburn.Micro;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Common.Model.Setup;
using System.Threading.Tasks.Dataflow;
using System.Linq;
using System.Collections.Generic;

namespace TickTrader.BotTerminal
{
    internal class TradeBotModel2 : PluginModel
    {
        private static readonly INameGenerator _uniqueNameGenerator = new UniqueNameGenerator();

        public TradeBotModel2(PluginSetup pSetup, IAlgoPluginHost host)
            : base(pSetup, host)
        {
            Name = _uniqueNameGenerator.GenerateFrom(Name);

            host.Journal.RegisterBotLog(Name);
        }

        public void Start()
        {
            if (State != BotModelStates.Stopped)
                return;
            Host.Lock();
            if (StartExcecutor())
                ChangeState(BotModelStates.Running);
        }

        public async Task Stop()
        {
            if (State != BotModelStates.Running)
                return;
            ChangeState(BotModelStates.Stopping);
            await StopExecutor();
            ChangeState(BotModelStates.Stopped);
            Host.Unlock();
        }

        public void Remove()
        {
            Host.Journal.UnregisterBotLog(Name);
            Removed(this);
        }

        private void ChangeState(BotModelStates newState)
        {
            State = newState;
            StateChanged(this);
        }

        public BotModelStates State { get; private set; }
        public string CustomStatus { get; private set; }

        public event System.Action<TradeBotModel2> CustomStatusChanged = delegate { };
        public event System.Action<TradeBotModel2> StateChanged = delegate { };
        public event System.Action<TradeBotModel2> Removed = delegate { };

        protected override PluginExecutor CreateExecutor()
        {            
            var executor = base.CreateExecutor();
            executor.IsRunningChanged += Executor_IsRunningChanged;
            executor.TradeApi = Host.GetTradeApi();
            executor.InitLogging().NewRecords += TradeBotModel2_NewRecords;
            return executor;
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

        private BotMessage Convert(BotLogRecord record)
        {
            return new BotMessage(record.Time, Name, record.Message, Convert(record.Severity)) { Details = record.Details };
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

        private void Executor_IsRunningChanged(PluginExecutor pluginExecutor)
        {
            Execute.OnUIThread(() =>
            {
                if (!pluginExecutor.IsRunning && State == BotModelStates.Running)
                {
                    ChangeState(BotModelStates.Stopped);
                    Host.Unlock();
                }
            });
        }
    }
}
