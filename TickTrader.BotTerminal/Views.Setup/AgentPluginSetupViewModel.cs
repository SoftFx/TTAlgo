using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using System.Threading;

namespace TickTrader.BotTerminal
{
    internal class AgentPluginSetupViewModel : Screen, IWindowModel
    {
        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private bool _runBot;
        private AlgoEnvironment _algoEnv;
        private AlgoAgentViewModel _selectedAgent;
        private AlgoAccountViewModel _selectedAccount;
        private AlgoPluginViewModel _selectedPlugin;
        private CancellationTokenSource _updateSetupMetadataSrc;
        private TaskCompletionSource<SetupMetadata> _updateSetupMetadataTaskSrc;
        private CancellationTokenSource _updateSetupCancelSrc;
        private bool _hasPendingRequest;
        private string _requestError;
        private bool _showFileProgress;


        public IObservableList<AlgoAgentViewModel> Agents { get; }

        public AlgoAgentViewModel SelectedAgent
        {
            get { return _selectedAgent; }
            set
            {
                if (_selectedAgent == value)
                    return;

                if (_selectedAgent != null)
                {
                    _selectedAgent.Plugins.Updated -= AllPlugins_Updated;
                    _selectedAgent.Model.BotStateChanged -= BotStateChanged;
                    _selectedAgent.Model.AccessLevelChanged -= OnAccessLevelChanged;
                }
                _selectedAgent = value;
                NotifyOfPropertyChange(nameof(SelectedAgent));
                InitAgent();
                _selectedAgent.Plugins.Updated += AllPlugins_Updated;
                _selectedAgent.Model.BotStateChanged += BotStateChanged;
                _selectedAgent.Model.AccessLevelChanged += OnAccessLevelChanged;
            }
        }

        public SetupContextInfo SetupContext { get; }

        public IObservableList<AlgoAccountViewModel> Accounts { get; private set; }

        public AlgoAccountViewModel SelectedAccount
        {
            get { return _selectedAccount; }
            set
            {
                if (_selectedAccount == value)
                    return;

                _selectedAccount = value;
                NotifyOfPropertyChange(nameof(SelectedAccount));
                UpdateSetupMetadata();
                UpdateSetup();
            }
        }

        public IObservableList<AlgoPluginViewModel> Plugins { get; private set; }

        public AlgoPluginViewModel SelectedPlugin
        {
            get { return _selectedPlugin; }
            set
            {
                if (_selectedPlugin == value)
                    return;

                _selectedPlugin = value;
                NotifyOfPropertyChange(nameof(SelectedPlugin));
                UpdateSetup();
            }
        }

        public PluginConfigViewModel Setup { get; private set; }

        public ITradeBot Bot { get; private set; }

        public bool PluginIsStopped => Bot == null ? true : PluginStateHelper.IsStopped(Bot.State);

        public bool CanOk => (Setup?.IsValid ?? false) && PluginIsStopped && !_hasPendingRequest
            && IsNewMode ? SelectedAgent.Model.AccessManager.CanAddBot() : SelectedAgent.Model.AccessManager.CanChangeBotConfig();

        public bool RunBot
        {
            get { return _runBot; }
            set
            {
                if (_runBot == value)
                    return;

                _runBot = value;
                NotifyOfPropertyChange(nameof(RunBot));
            }
        }

        public AlgoTypes Type { get; }

        public string PluginType { get; }

        public PluginSetupMode Mode { get; }

        public bool IsNewMode => Mode == PluginSetupMode.New;

        public bool IsEditMode => Mode == PluginSetupMode.Edit;

        public bool HasPendingRequest
        {
            get { return _hasPendingRequest; }
            set
            {
                if (_hasPendingRequest == value)
                    return;

                _hasPendingRequest = value;
                NotifyOfPropertyChange(nameof(HasPendingRequest));
                NotifyOfPropertyChange(nameof(CanOk));
            }
        }

        public string RequestError
        {
            get { return _requestError; }
            set
            {
                if (_requestError == value)
                    return;

                _requestError = value;
                NotifyOfPropertyChange(nameof(RequestError));
            }
        }

        public bool ShowFileProgress
        {
            get { return _showFileProgress; }
            set
            {
                if (_showFileProgress == value)
                    return;

                _showFileProgress = value;
                NotifyOfPropertyChange(nameof(ShowFileProgress));
            }
        }

        public ProgressViewModel FileProgress { get; }


        private AgentPluginSetupViewModel(AlgoEnvironment algoEnv, string agentName, AccountKey accountKey, PluginKey pluginKey, AlgoTypes type, SetupContextInfo setupContext, PluginSetupMode mode)
        {
            _algoEnv = algoEnv;
            Mode = mode;
            Type = type;
            SetupContext = setupContext;

            Agents = algoEnv.Agents.AsObservable();
            SelectedAgent = Agents.FirstOrDefault(a => a.Name == agentName) ?? (Agents.Any() ? Agents.First() : null);
            SelectedAccount = Accounts.FirstOrDefault(a => a.Key.Equals(accountKey)) ?? (Accounts.Any() ? Accounts.First() : null);
            SelectedPlugin = Plugins.FirstOrDefault(i => i.Key.Equals(pluginKey)) ?? (Plugins.Any() ? Plugins.First() : null);

            PluginType = GetPluginTypeDisplayName(Type);

            ShowFileProgress = false;
            FileProgress = new ProgressViewModel();

            RunBot = true;
        }

        public AgentPluginSetupViewModel(AlgoEnvironment algoEnv, string agentName, AccountKey accountKey, PluginKey pluginKey, AlgoTypes type, SetupContextInfo setupContext)
            : this(algoEnv, agentName, accountKey, pluginKey, type, setupContext, PluginSetupMode.New)
        {
            DisplayName = $"Setting New {PluginType}";
        }

        public AgentPluginSetupViewModel(AlgoEnvironment algoEnv, string agentName, ITradeBot bot)
            : this(algoEnv, agentName, bot.Account, bot.Config.Key, AlgoTypes.Robot, null, PluginSetupMode.Edit)
        {
            Bot = bot;
            UpdateSetup();

            DisplayName = $"Settings - {bot.InstanceId}";
        }


        public void Reset()
        {
            Setup.Reset();
        }

        public async void Ok()
        {
            var config = GetConfig();
            RequestError = null;
            HasPendingRequest = true;
            try
            {
                if (Type == AlgoTypes.Robot && Mode == PluginSetupMode.New)
                {
                    if (!SelectedAgent.Model.Bots.Snapshot.ContainsKey(config.InstanceId))
                        await SelectedAgent.Model.AddBot(SelectedAccount.Key, config);
                    else await SelectedAgent.Model.ChangeBotConfig(config.InstanceId, config);
                    await UploadBotFiles(config);
                    SelectedAgent.OpenBotState(config.InstanceId);
                    if (RunBot)
                        await SelectedAgent.Model.StartBot(config.InstanceId);
                }
                else
                {
                    await SelectedAgent.Model.ChangeBotConfig(Bot.InstanceId, config);
                    await UploadBotFiles(config);
                }
                TryClose();
            }
            catch (Exception ex)
            {
                RequestError = ex.Message;
            }
            HasPendingRequest = false;
        }

        public void Cancel()
        {
            TryClose();
        }

        public override void CanClose(Action<bool> callback)
        {
            callback(true);
            Dispose();
        }

        public PluginConfig GetConfig()
        {
            var res = Setup.Save();
            res.Key = SelectedPlugin.Key;
            return res;
        }


        private void BotStateChanged(ITradeBot bot)
        {
            if (Bot != null && Bot.InstanceId == bot.InstanceId)
            {
                Bot = bot;
                NotifyOfPropertyChange(nameof(PluginIsStopped));
                NotifyOfPropertyChange(nameof(CanOk));
            }
        }

        private void OnAccessLevelChanged()
        {
            NotifyOfPropertyChange(nameof(CanOk));
        }

        private void InitAgent()
        {
            Accounts = SelectedAgent.Accounts.AsObservable();
            Plugins = SelectedAgent.Plugins.Where(p => p.Descriptor.Type == Type).AsObservable();
            NotifyOfPropertyChange(nameof(Accounts));
            NotifyOfPropertyChange(nameof(Plugins));
            SelectedAccount = (Accounts.Any() ? Accounts.First() : null);
            SelectedPlugin = (Plugins.Any() ? Plugins.First() : null);
        }

        private void Init()
        {
            Setup.ValidityChanged += Validate;
            Validate();

            _logger.Debug($"Init {Setup.Descriptor.DisplayName} "
                 + Setup.Parameters.Count() + " params "
                 + Setup.Inputs.Count() + " inputs "
                 + Setup.Outputs.Count() + " outputs ");
        }

        private void Validate()
        {
            NotifyOfPropertyChange(nameof(CanOk));
        }

        private void AllPlugins_Updated(ListUpdateArgs<AlgoPluginViewModel> args)
        {
            if (SelectedPlugin == null)
                return;

            if (args.Action == DLinqAction.Replace)
            {
                if (args.NewItem.Key.Equals(SelectedPlugin.Key))
                {
                    SelectedPlugin = args.NewItem;
                }
            }
            else if (args.Action == DLinqAction.Remove)
            {
                if (args.OldItem.Key.Equals(SelectedPlugin.Key))
                    TryClose();
            }
        }

        private void Dispose()
        {
            if (SelectedAgent != null)
            {
                _selectedAgent.Plugins.Updated -= AllPlugins_Updated;
                _selectedAgent.Model.BotStateChanged -= BotStateChanged;
                _selectedAgent.Model.AccessLevelChanged -= OnAccessLevelChanged;
            }
            if (Setup != null)
                Setup.ValidityChanged -= Validate;
        }

        private string GetPluginTypeDisplayName(AlgoTypes type)
        {
            switch (type)
            {
                case AlgoTypes.Robot: return "Bot";
                case AlgoTypes.Indicator: return "Indicator";
                default: return "PluginType";
            }
        }

        private async void UpdateSetupMetadata()
        {
            if (SelectedAccount != null)
            {
                _updateSetupMetadataSrc?.Cancel();
                _updateSetupMetadataSrc = new CancellationTokenSource();

                var tcs = new TaskCompletionSource<SetupMetadata>();
                _updateSetupMetadataTaskSrc = tcs;

                var metadata = await SelectedAgent.Model.GetSetupMetadata(SelectedAccount.Key, SetupContext);

                if (_updateSetupMetadataSrc.IsCancellationRequested)
                {
                    tcs.SetCanceled();
                    return;
                }

                tcs.SetResult(metadata);
            }
        }

        private async void UpdateSetup()
        {
            if (SelectedPlugin != null && _updateSetupMetadataSrc != null)
            {
                _updateSetupCancelSrc?.Cancel();
                _updateSetupCancelSrc = new CancellationTokenSource();

                SetupMetadata metadata = null;
                try
                {
                    metadata = await _updateSetupMetadataTaskSrc.Task;
                }
                catch (TaskCanceledException)
                {
                    UpdateSetup();
                    return;
                }

                if (_updateSetupCancelSrc.IsCancellationRequested)
                    return;

                if (Setup != null)
                    Setup.ValidityChanged -= Validate;
                Setup = new PluginConfigViewModel(SelectedPlugin.Info, metadata, SelectedAgent.Model.IdProvider, Mode);
                Init();
                if (Bot != null)
                    Setup.Load(Bot.Config);

                NotifyOfPropertyChange(nameof(Setup));
            }
        }

        private async Task UploadBotFiles(PluginConfig config)
        {
            ShowFileProgress = true;
            try
            {
                foreach (FileParameter fileParam in config.Properties.Where(p => p is FileParameter))
                {
                    var path = fileParam.FileName;
                    if (System.IO.File.Exists(path) && System.IO.Path.GetFullPath(path) == path)
                    {
                        var fileInfo = new System.IO.FileInfo(path);
                        FileProgress.SetMessage($"Uploading {fileInfo.Name} to AlgoData...");
                        var fileProgressListener = new FileProgressListenerAdapter(FileProgress, fileInfo.Length);
                        await SelectedAgent.Model.UploadBotFile(config.InstanceId, BotFolderId.AlgoData, fileInfo.Name, path, fileProgressListener);
                    }
                }
            }
            finally
            {
                ShowFileProgress = false;
            }
        }
    }
}
