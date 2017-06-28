using Caliburn.Micro;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Common.Model.Setup;
using System.IO;

namespace TickTrader.BotTerminal
{
    internal class TradeBotModel : PluginModel
    {
        public BotModelStates State { get; private set; }
        public string CustomStatus { get; private set; }


        public event System.Action<TradeBotModel> CustomStatusChanged = delegate { };
        public event System.Action<TradeBotModel> StateChanged = delegate { };
        public event System.Action<TradeBotModel> Removed = delegate { };


        public TradeBotModel(PluginSetupViewModel pSetup, IAlgoPluginHost host)
            : base(pSetup, host)
        {
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
            Removed(this);
        }


        protected override PluginExecutor CreateExecutor()
        {
            var executor = base.CreateExecutor();

            executor.IsRunningChanged += Executor_IsRunningChanged;

            executor.TradeApi = Host.GetTradeApi();
            executor.Logger = new LogAdapter(Host.Journal, InstanceId, s =>
            {
                CustomStatus = s;
                CustomStatusChanged(this);
            });
            executor.WorkingFolder = Path.Combine(EnvService.Instance.AlgoWorkingFolder, PathHelper.GetSafeFileName(InstanceId));
            executor.BotWorkingFolder = executor.WorkingFolder;
            EnvService.Instance.EnsureFolder(executor.WorkingFolder);

            return executor;
        }


        private void ChangeState(BotModelStates newState)
        {
            State = newState;
            StateChanged(this);
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
            private Action<string> _statusChanged;
            private BotJournal _journal;
            private string _botId;


            public LogAdapter(BotJournal journal, string botId, Action<string> statusChangedHandler)
            {
                this._journal = journal;
                this._botId = botId;
                this._statusChanged = statusChangedHandler;
            }


            public void UpdateStatus(string status)
            {
                _statusChanged(status);
            }

            public void OnPrint(string entry)
            {
                _journal.Custom(_botId, entry);
            }

            public void OnPrint(string entry, object[] parameters)
            {
                string msg = entry;
                try
                {
                    msg = string.Format(entry, parameters);
                }
                catch { }

                _journal.Custom(_botId, msg);
            }

            public void OnPrintError(string entry)
            {
                _journal.Error(_botId, entry);
            }

            public void OnPrintError(string entry, object[] parameters)
            {
                string msg = entry;
                try
                {
                    msg = string.Format(entry, parameters);
                }
                catch { }

                _journal.Error(_botId, msg);
            }

            public void OnPrintInfo(string entry)
            {
                _journal.Info(_botId, entry);
            }

            public void OnError(Exception ex)
            {
                _journal.Error(_botId, "Exception: " + ex.Message);
            }

            public void OnInitialized()
            {
                _journal.Info(_botId, "Bot initialized");
            }

            public void OnStart()
            {
                _journal.Info(_botId, "Bot started");
            }

            public void OnStop()
            {
                _journal.Info(_botId, "Bot stopped");
            }

            public void OnExit()
            {
                _journal.Info(_botId, "Bot exited");
            }

            public void OnPrintTrade(string entry)
            {
                _journal.Trading(_botId, entry);
            }
        }
    }
}
