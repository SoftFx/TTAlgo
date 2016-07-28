using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.GuiModel;

namespace TickTrader.BotTerminal
{
    internal class TradeBotModel2 : PluginModel
    {
        public TradeBotModel2(PluginSetup pSetup, IAlgoPluginHost host)
            : base(pSetup, host)
        {
        }

        public async Task Start()
        {
            if (State != BotModelStates.Stopped)
                return;
            ChangeState(BotModelStates.Starting);
            Host.Lock();
            await StartExcecutor();
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
            Removed(this);
        }

        private void ChangeState(BotModelStates newState)
        {
            State = newState;
            StateChanged(this);
        }

        public BotModelStates State { get; private set; }
        public string CustomStatus { get; private set; }

        public event System.Action<TradeBotModel2> CustomStatusChnaged = delegate { };
        public event System.Action<TradeBotModel2> StateChanged = delegate { };
        public event System.Action<TradeBotModel2> Removed = delegate { };

        protected override PluginExecutor CreateExecutor()
        {
            var executor = base.CreateExecutor();
            executor.IsRunningChanged += Executor_IsRunningChanged;
            executor.Logger = new LogAdapter(Host.Journal, Name, s =>
            {
                CustomStatus = s;
                CustomStatusChnaged(this);
            });
            return executor;
        }

        private void Executor_IsRunningChanged(PluginExecutor pluginExecutor)
        {
            Execute.OnUIThread(() =>
            {
                if (!pluginExecutor.IsRunning && State == BotModelStates.Running)
                    ChangeState(BotModelStates.Stopped);
            });
        }

        private class LogAdapter : NoTimeoutByRefObject, IPluginLogger
        {
            private Action<string> statusChanged;
            private BotJournal journal;
            private string botName;

            public LogAdapter(BotJournal journal, string botName, Action<string> statusChangedHandler)
            {
                this.journal = journal;
                this.botName = botName;
                this.statusChanged = statusChangedHandler;
            }

            public void UpdateStatus(string status)
            {
                statusChanged(status);
            }

            public void WriteLog(string entry, object[] parameters)
            {
                string msg = entry;
                try
                {
                    msg = string.Format(entry, parameters);
                }
                catch { }

                journal.Info(botName, msg);
            }

            public void WriteError(string msg, string description)
            {
                journal.Error(botName, msg);
            }
        }
    }
}
