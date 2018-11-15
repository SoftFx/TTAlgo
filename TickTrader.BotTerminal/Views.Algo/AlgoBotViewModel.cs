using Caliburn.Micro;
using System.Diagnostics;
using System.IO;
using TickTrader.Algo.Common.Info;

namespace TickTrader.BotTerminal
{
    internal class AlgoBotViewModel : PropertyChangedBase
    {
        public ITradeBot Model { get; }

        public AlgoAgentViewModel Agent { get; }


        public string InstanceId => Model.InstanceId;

        public AccountKey Account => Model.Account;

        public PluginStates State => Model.State;

        public bool IsRunning => PluginStateHelper.IsRunning(Model.State);

        public bool IsStopped => PluginStateHelper.IsStopped(Model.State);

        public bool CanStart => PluginStateHelper.IsStopped(Model.State) && Agent.Model.AccessManager.CanStartBot();

        public bool CanStop => PluginStateHelper.IsRunning(Model.State) && Agent.Model.AccessManager.CanStopBot();

        public bool CanStartStop => CanStart || CanStop;

        public bool CanRemove => PluginStateHelper.IsStopped(Model.State) && Agent.Model.AccessManager.CanRemoveBot();

        public bool CanOpenChart => !Model.IsRemote && (Model.Descriptor?.SetupMainSymbol ?? false);

        public string Status => Model.Status;

        public bool CanBrowse => !Model.IsRemote || Agent.Model.AccessManager.CanGetBotFolderInfo(BotFolderId.BotLogs);

        public bool CanCopyTo => Agent.Model.AccessManager.CanDownloadPackage() && Agent.Model.AccessManager.CanGetBotFolderInfo(BotFolderId.AlgoData) && Agent.Model.AccessManager.CanDownloadBotFile(BotFolderId.AlgoData);


        public AlgoBotViewModel(ITradeBot bot, AlgoAgentViewModel agent)
        {
            Model = bot;
            Agent = agent;

            Model.StateChanged += OnStateChanged;
            Model.Updated += OnUpdated;
            Model.StatusChanged += OnStatusChanged;
            Agent.Model.AccessLevelChanged += OnAccessLevelChanged;
        }


        public void Start()
        {
            Agent.StartBot(InstanceId).Forget();
        }

        public void Stop()
        {
            Agent.StopBot(InstanceId).Forget();
        }

        public void StartStop()
        {
            if (IsRunning)
                Stop();
            else if (IsStopped)
                Start();
        }

        public void Remove()
        {
            Agent.RemoveBot(InstanceId).Forget();
        }

        public void OpenSettings()
        {
            Agent.OpenBotSetup(Model);
        }

        public void OpenChart()
        {
            Agent.ShowChart(Model);
        }

        public void OpenState()
        {
            Agent.OpenBotState(InstanceId);
        }

        public void Browse()
        {
            if (Model.IsRemote)
            {
                Agent.OpenManageBotFilesDialog(InstanceId, BotFolderId.BotLogs);
            }
            else
            {
                var logDir = Path.Combine(EnvService.Instance.BotLogFolder, PathHelper.GetSafeFileName(InstanceId));

                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);
                Process.Start(logDir);
            }
        }

        public void CopyTo()
        {
            Agent.OpenCopyBotInstanceDialog(InstanceId);
        }


        private void OnStateChanged(ITradeBot bot)
        {
            NotifyOfPropertyChange(nameof(State));
            NotifyOfPropertyChange(nameof(IsRunning));
            NotifyOfPropertyChange(nameof(IsStopped));
            NotifyOfPropertyChange(nameof(CanStart));
            NotifyOfPropertyChange(nameof(CanStop));
            NotifyOfPropertyChange(nameof(CanStartStop));
            NotifyOfPropertyChange(nameof(CanRemove));
        }

        private void OnUpdated(ITradeBot bot)
        {
        }

        private void OnStatusChanged(ITradeBot bot)
        {
            NotifyOfPropertyChange(nameof(Status));
        }

        private void OnAccessLevelChanged()
        {
            NotifyOfPropertyChange(nameof(CanStart));
            NotifyOfPropertyChange(nameof(CanStop));
            NotifyOfPropertyChange(nameof(CanStartStop));
            NotifyOfPropertyChange(nameof(CanRemove));
            NotifyOfPropertyChange(nameof(CanBrowse));
            NotifyOfPropertyChange(nameof(CanCopyTo));
        }
    }
}
