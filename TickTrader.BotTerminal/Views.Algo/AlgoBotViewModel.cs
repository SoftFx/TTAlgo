using Caliburn.Micro;
using System;
using System.Diagnostics;
using System.IO;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.ServerControl;

namespace TickTrader.BotTerminal
{
    internal class AlgoBotViewModel : PropertyChangedBase, IDisposable
    {
        public ITradeBot Model { get; }

        public AlgoAgentViewModel Agent { get; }

        public string InstanceId => Model.InstanceId;

        public string AccountId => Model.AccountId;

        public PluginModelInfo.Types.PluginState State => Model.State;

        public PluginKey Plugin => Model.Config.Key;

        public bool IsRunning => Model.State.IsRunning() || Agent.Model.IsRemote && Model.State == PluginModelInfo.Types.PluginState.Starting;

        public bool IsStopped => Model.State.IsStopped();

        public bool CanStart => IsStopped && Agent.Model.AccessManager.CanStartPlugin();

        public bool CanStop => IsRunning && Agent.Model.AccessManager.CanStopPlugin();

        public bool CanStartStop => CanStart || CanStop;

        public bool CanRemove => Model.State.IsStopped() && Agent.Model.AccessManager.CanRemovePlugin();

        public bool CanOpenChart => !Model.IsRemote && (Model.Descriptor?.SetupMainSymbol ?? false);

        public string Status => Model.Status;

        public bool CanBrowse => !Model.IsRemote || Agent.Model.AccessManager.CanGetBotFolderInfo(PluginFolderInfo.Types.PluginFolderId.BotLogs.ToApi());

        public bool CanCopyTo => Agent.Model.AccessManager.CanDownloadPackage() && Agent.Model.AccessManager.CanGetBotFolderInfo(PluginFolderInfo.Types.PluginFolderId.AlgoData.ToApi())
            && Agent.Model.AccessManager.CanDownloadBotFile(PluginFolderInfo.Types.PluginFolderId.AlgoData.ToApi());

        public bool CanAddBot => Agent.Model.AccessManager.CanAddPlugin();

        public AlgoBotViewModel(ITradeBot bot, AlgoAgentViewModel agent)
        {
            Model = bot;
            Agent = agent;

            Model.StateChanged += OnStateChanged;
            Model.Updated += OnUpdated;
            Model.StatusChanged += OnStatusChanged;
            Agent.Model.AccessLevelChanged += OnAccessLevelChanged;
        }


        public void Dispose()
        {
            Model.StateChanged -= OnStateChanged;
            Model.Updated -= OnUpdated;
            Model.StatusChanged -= OnStatusChanged;
            Agent.Model.AccessLevelChanged -= OnAccessLevelChanged;
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
                Agent.OpenManageBotFilesDialog(InstanceId, PluginFolderInfo.Types.PluginFolderId.BotLogs);
            }
            else
            {
                var logDir = Path.Combine(EnvService.Instance.BotLogFolder, PathHelper.Escape(InstanceId));

                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);
                Process.Start(logDir);
            }
        }

        public void CopyTo()
        {
            Agent.OpenCopyBotInstanceDialog(InstanceId);
        }

        public void AddBot()
        {
            Agent.OpenBotSetup(Model.AccountId, Model.Config.Key);
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
            NotifyOfPropertyChange(nameof(CanAddBot));
        }
    }
}
