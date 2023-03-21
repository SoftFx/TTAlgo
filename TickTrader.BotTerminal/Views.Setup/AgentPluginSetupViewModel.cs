using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal sealed class AgentPluginSetupViewModel : Screen, IWindowModel, IDisposable
    {
        private const string DefaultDateTimeFormat = "dd.MM.yyyy hh:mm:ss";

        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private AlgoAgentViewModel _selectedAgent;
        private AlgoAccountViewModel _selectedAccount;
        private AlgoPluginViewModel _selectedPlugin;
        private CancellationTokenSource _updateSetupMetadataSrc;
        private TaskCompletionSource<SetupMetadata> _updateSetupMetadataTaskSrc;
        private CancellationToken _updateSetupToken;

        private string _requestError;
        private bool _hasPendingRequest;
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
                NotifyOfPropertyChange(nameof(IsNotTerminal));
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

        public List<string> AvailableBots { get; private set; }

        public List<AlgoPluginViewModel> SelectedPluginVersions { get; private set; }


        private string _selectedPluginName;

        public string SelectedPluginName
        {
            get => _selectedPluginName;

            set
            {
                if (_selectedPluginName == value)
                    return;

                _selectedPluginName = value;

                SelectedPlugin = Plugins.Where(x => x.DisplayName == _selectedPluginName).OrderByDescending(u => u.Descriptor.Version).FirstOrDefault();

                NotifyOfPropertyChange();
            }
        }

        public AlgoPluginViewModel SelectedPlugin
        {
            get => _selectedPlugin;
            set
            {
                if (_selectedPlugin == value || value is null)
                    return;

                _selectedPlugin = value;

                SelectedPluginVersions = Plugins.Where(u => u.DisplayName == _selectedPlugin.DisplayName).ToList();
                _selectedPluginName = _selectedPlugin.DisplayName;

                SelectedPluginPackageId = _selectedPlugin.PackageInfo.PackageId;
                SelectedPluginLastModify = _selectedPlugin.PackageInfo.Identity.LastModifiedUtc?.ToDateTime().ToString(DefaultDateTimeFormat);
                SelectedPluginPackageSize = $"{_selectedPlugin.PackageInfo.Identity.Size / 1024} KB";

                NotifyOfPropertyChange(nameof(SelectedPlugin));
                NotifyOfPropertyChange(nameof(SelectedPluginName));
                NotifyOfPropertyChange(nameof(SelectedPluginVersions));

                NotifyOfPropertyChange(nameof(SelectedPluginPackageId));
                NotifyOfPropertyChange(nameof(SelectedPluginLastModify));
                NotifyOfPropertyChange(nameof(SelectedPluginPackageSize));

                NotifyOfPropertyChange(nameof(CanOk));
                UpdateSetup();
            }
        }

        public string SelectedPluginPackageId { get; private set; }

        public string SelectedPluginLastModify { get; private set; }

        public string SelectedPluginPackageSize { get; private set; }


        public PluginConfigViewModel Setup { get; private set; }

        public ITradeBot Bot { get; private set; }

        public bool PluginIsStopped => Bot == null || Bot.State.IsStopped();

        public bool CanOk => (Setup?.IsValid ?? false) && PluginIsStopped && !_hasPendingRequest && SelectedPlugin != null
            && (IsNewMode ? SelectedAgent.Model.AccessManager.CanAddPlugin() : SelectedAgent.Model.AccessManager.CanChangePluginConfig());

        public bool IsNotTerminal => Mode == PluginSetupMode.New && SelectedAgent.Name != LocalAlgoAgent.LocalAgentName;

        public Metadata.Types.PluginType Type { get; }

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

        public ProgressViewModel FileProgress { get; } = new();


        private AgentPluginSetupViewModel(AlgoEnvironment algoEnv, string agentName, string accountId, PluginKey pluginKey, Metadata.Types.PluginType type, SetupContextInfo setupContext, PluginSetupMode mode)
        {
            Mode = mode;
            Type = type;
            SetupContext = setupContext;

            Agents = algoEnv.Agents.AsObservable();
            SelectedAgent = Agents.FirstOrDefault(a => a.Name == agentName) ?? Agents.FirstOrDefault();
            SelectedAccount = Accounts.FirstOrDefault(a => a.AccountId.Equals(accountId)) ?? Accounts.FirstOrDefault();
            SelectedPlugin = Plugins.FirstOrDefault(i => i.Key.Equals(pluginKey)) ?? Plugins.FirstOrDefault();
            PluginType = GetPluginTypeDisplayName(Type);

            ShowFileProgress = false;
        }

        public AgentPluginSetupViewModel(AlgoEnvironment algoEnv, string agentName, string accountId, PluginKey pluginKey, Metadata.Types.PluginType type, SetupContextInfo setupContext)
            : this(algoEnv, agentName, accountId, pluginKey, type, setupContext, PluginSetupMode.New)
        {
            DisplayName = Type == Metadata.Types.PluginType.TradeBot ? $"New Bot Instance" : $"Setting New {PluginType}";
        }

        public AgentPluginSetupViewModel(AlgoEnvironment algoEnv, string agentName, ITradeBot bot)
            : this(algoEnv, agentName, bot.AccountId, bot.Config.Key, Metadata.Types.PluginType.TradeBot, null, PluginSetupMode.Edit)
        {
            Bot = bot;
            UpdateSetup();

            DisplayName = $"{bot.InstanceId} Bot Instance";
        }


        public void AddNewAccount() => SelectedAgent.UpdatePluginAccountSettings(this);

        public void UploadNewPlugin() => SelectedAgent.OpenUploadPackageDialog();

        public void Reset()
        {
            Setup.Reset();
        }

        public async Task Ok()
        {
            var config = GetConfig();
            var algoServer = SelectedAgent.Model;

            RequestError = null;
            HasPendingRequest = true;

            try
            {
                var fileUploadList = algoServer.IsRemote ? config.FixFileParametersForRemote() : null;

                if (!algoServer.Bots.Snapshot.ContainsKey(config.InstanceId))
                    await algoServer.AddBot(SelectedAccount.AccountId, config);
                else
                    await algoServer.ChangeBotConfig(config.InstanceId, config);

                await UploadBotFiles(config, fileUploadList);

                if (Type == Metadata.Types.PluginType.TradeBot && Mode == PluginSetupMode.New)
                {
                    SelectedAgent.OpenBotState(config.InstanceId);

                    if (Setup.RunBot)
                        await algoServer.StartBot(config.InstanceId);
                }

                await TryCloseAsync();
            }
            catch (Exception ex)
            {
                RequestError = ex.Message;
            }

            HasPendingRequest = false;
        }

        public Task Cancel() => TryCloseAsync();

        public override Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
        {
            Dispose();
            return base.CanCloseAsync(cancellationToken);
        }

        public PluginConfig GetConfig()
        {
            var res = Setup.Save();
            res.Key = SelectedPlugin.Key;
            return res;
        }

        public void SetNewAccount(string login)
        {
            SelectedAccount = Accounts.FirstOrDefault(u => u.Login == login) ?? Accounts.FirstOrDefault();
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
            AvailableBots = Plugins.DistinctBy(u => u.Descriptor.DisplayName).Select(u => u.DisplayName).ToList();

            NotifyOfPropertyChange(nameof(Accounts));
            NotifyOfPropertyChange(nameof(Plugins));
            NotifyOfPropertyChange(nameof(AvailableBots));

            SelectedAccount = Accounts.FirstOrDefault();
            SelectedPlugin = Plugins.FirstOrDefault();
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

        private async void AllPlugins_Updated(ListUpdateArgs<AlgoPluginViewModel> args)
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
                    await TryCloseAsync();
            }
        }

        public void Dispose()
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

        private static string GetPluginTypeDisplayName(Metadata.Types.PluginType type)
        {
            return type switch
            {
                Metadata.Types.PluginType.TradeBot => "Bot",
                Metadata.Types.PluginType.Indicator => "Indicator",
                _ => "PluginType",
            };
        }

        private async void UpdateSetupMetadata()
        {
            if (SelectedAccount != null)
            {
                _updateSetupMetadataSrc?.Cancel();

                _updateSetupMetadataSrc = new CancellationTokenSource();
                _updateSetupToken = _updateSetupMetadataSrc.Token;

                var tcs = new TaskCompletionSource<SetupMetadata>();
                _updateSetupMetadataTaskSrc = tcs;

                var metadata = await SelectedAgent.Model.GetSetupMetadata(SelectedAccount.AccountId, SetupContext);

                if (_updateSetupToken.IsCancellationRequested)
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
                var currentUpdateSetupToken = _updateSetupToken;

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

                if (currentUpdateSetupToken.IsCancellationRequested)
                    return;

                if (Setup != null)
                    Setup.ValidityChanged -= Validate;

                Setup = new PluginConfigViewModel(SelectedPlugin.PluginInfo, metadata, SelectedAgent.Model.IdProvider, Mode);
                Init();

                if (Bot != null)
                    Setup.Load(Bot.Config);

                NotifyOfPropertyChange(nameof(Setup));
            }
            else if (Setup != null)
                Setup.Visible = false;
        }

        private async Task UploadBotFiles(PluginConfig config, List<(string, string)> fileUploadList)
        {
            if (fileUploadList == null || fileUploadList.Count == 0)
                return;

            ShowFileProgress = true;
            try
            {
                foreach (var (_, path) in fileUploadList)
                {
                    if (System.IO.File.Exists(path) && System.IO.Path.GetFullPath(path) == path)
                    {
                        var fileInfo = new System.IO.FileInfo(path);
                        FileProgress.SetMessage($"Uploading {fileInfo.Name} to AlgoData...");
                        var fileProgressListener = new FileProgressListenerAdapter(FileProgress, fileInfo.Length);
                        await SelectedAgent.Model.UploadBotFile(config.InstanceId, PluginFolderInfo.Types.PluginFolderId.AlgoData, fileInfo.Name, path, fileProgressListener);
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
