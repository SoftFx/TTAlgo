using Caliburn.Micro;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.GuiModel;

namespace TickTrader.BotTerminal
{
    internal class TradeBotModel2 : PluginModel
    {
        private static readonly INameGenerator _uniqueNameGenerator = new UniqueNameGenerator();

        public TradeBotModel2(PluginSetup pSetup, IAlgoPluginHost host)
            : base(pSetup, host)
        {
        }

        public void Start()
        {
            if (State != BotModelStates.Stopped)
                return;
            Host.Lock();
            StartExcecutor();
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

        public event System.Action<TradeBotModel2> CustomStatusChanged = delegate { };
        public event System.Action<TradeBotModel2> StateChanged = delegate { };
        public event System.Action<TradeBotModel2> Removed = delegate { };

        protected override PluginExecutor CreateExecutor()
        {
            Name = _uniqueNameGenerator.GenerateFrom(Name); //Dirty Workaround

            var executor = base.CreateExecutor();
            executor.IsRunningChanged += Executor_IsRunningChanged;
            executor.TradeApi = Host.GetTradeApi();
            executor.Logger = new LogAdapter(Host.Journal, Name, s =>
            {
                CustomStatus = s;
                CustomStatusChanged(this);
            });
            return executor;
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

        private class LogAdapter : CrossDomainObject, IPluginLogger
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

            public void OnPrint(string entry, object[] parameters)
            {
                string msg = entry;
                try
                {
                    msg = string.Format(entry, parameters);
                }
                catch { }

                journal.Info(botName, msg);
            }

            public void OnPrintError(string entry, object[] parameters)
            {
                string msg = entry;
                try
                {
                    msg = string.Format(entry, parameters);
                }
                catch { }

                journal.Error(botName, msg);
            }

            public void OnPrintInfo(string entry)
            {
                journal.Info(botName, entry);
            }

            public void OnError(Exception ex)
            {
                journal.Error(botName, "Exception: " + ex.Message);
            }

            public void OnInitialized()
            {
                journal.Info(botName, "Bot initialized");
            }

            public void OnStart()
            {
                journal.Info(botName, "Bot started");
            }

            public void OnStop()
            {
                journal.Info(botName, "Bot stopped");
            }

            public void OnExit()
            {
                journal.Info(botName, "Bot exited");
            }
        }
    }
}
