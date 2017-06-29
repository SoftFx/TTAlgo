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


        protected override PluginExecutor CreateExecutor()
        {
            var executor = base.CreateExecutor();
            executor.IsRunningChanged += Executor_IsRunningChanged;

            executor.TradeApi = Host.GetTradeApi();
            executor.WorkingFolder = Path.Combine(EnvService.Instance.AlgoWorkingFolder, PathHelper.GetSafeFileName(InstanceId));
            executor.BotWorkingFolder = executor.WorkingFolder;
            EnvService.Instance.EnsureFolder(executor.WorkingFolder);

            executor.InitLogging().NewRecords += TradeBotModel2_NewRecords;
            return executor;
        }


        private void ChangeState(BotModelStates newState)
        private void TradeBotModel2_NewRecords(BotLogRecord[] records)
        {
            State = newState;
            StateChanged(this);
        }

        {
            List<BotMessage> messages = new List<BotMessage>(records.Length);
            string status = null;

        private class LogAdapter : CrossDomainObject, IPluginLogger
        {
            private Action<string> statusChanged;
            private BotJournal journal;
            private string botName;

            public LogAdapter(BotJournal journal, string botName, Action<string> statusChangedHandler)

        private class LogAdapter : CrossDomainObject, IPluginLogger
        {
            private Action<string> _statusChanged;
            private BotJournal _journal;
            private string _botId;


            public LogAdapter(BotJournal journal, string botId, Action<string> statusChangedHandler)
            foreach (var rec in records)
            {
                this.journal = journal;
                this.botName = botName;
                this.statusChanged = statusChangedHandler;
                this._journal = journal;
                this._botId = botId;
                this._statusChanged = statusChangedHandler;
                if (rec.Severity != LogSeverities.CustomStatus)
                    messages.Add(Convert(rec));
                else
                    status = rec.Message;
            }


            public void UpdateStatus(string status)
            {
                statusChanged(status);
            }
            public void UpdateStatus(string status)
            {
                _statusChanged(status);
            }
            if (messages.Count > 0)
                Host.Journal.Add(messages);

            if (status != null)
            {
                journal.Custom(botName, entry);
                _journal.Custom(_botId, entry);
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

                journal.Custom(botName, msg);
            }

            public void OnPrintError(string entry)
                _journal.Custom(_botId, msg);
            }

            public void OnPrintError(string entry)
        private JournalMessageType Convert(LogSeverities severity)
        {
            switch (severity)
            {
                journal.Error(botName, entry);
                _journal.Error(_botId, entry);
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

            public void OnPrintTrade(string entry)
            {
                journal.Trading(botName, entry);
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
            });
        }
    }
}
