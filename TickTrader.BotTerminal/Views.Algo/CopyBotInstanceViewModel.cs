﻿using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.BotTerminal
{
    internal class CopyBotInstanceViewModel : Screen, IWindowModel
    {
        private readonly static ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        private AlgoEnvironment _algoEnv;
        private AlgoAgentViewModel _selectedAgent;
        private AlgoAccountViewModel _selectedAccount;
        private string _instanceId;
        private bool _isValid;
        private bool _hasPendingRequest;
        private string _error;
        private AlgoAgentViewModel _fromAgent;
        private string _fromBotId;
        private bool _fromBotRemoved;


        public string From { get; }

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
                if (_selectedAccount != null && _selectedAgent != _selectedAccount.Agent)
                {
                    DeinitAlgoAgent(_selectedAgent);
                    _selectedAgent = _selectedAccount.Agent;
                    InitAlgoAgent(_selectedAgent);
                    NotifyOfPropertyChange(nameof(IsInstanceIdValid));
                    NotifyOfPropertyChange(nameof(CanOk));
                }
                Validate();
            }
        }

        public string InstanceId
        {
            get { return _instanceId; }
            set
            {
                if (_instanceId == value)
                    return;

                _instanceId = value;
                NotifyOfPropertyChange(nameof(InstanceId));
                NotifyOfPropertyChange(nameof(IsInstanceIdValid));
                Validate();
            }
        }

        public bool IsInstanceIdValid => _selectedAgent != null && !string.IsNullOrEmpty(InstanceId) && _selectedAgent.Model.IdProvider.IsValidPluginId(AlgoTypes.Robot, InstanceId);

        public bool HasPendingRequest
        {
            get { return _hasPendingRequest; }
            set
            {
                if (_hasPendingRequest == value)
                    return;

                _hasPendingRequest = value;
                NotifyOfPropertyChange(nameof(HasPendingRequest));
                NotifyOfPropertyChange(nameof(IsEnabled));
                NotifyOfPropertyChange(nameof(CanOk));
            }
        }

        public bool IsEnabled => !_hasPendingRequest;

        public bool IsValid
        {
            get { return _isValid; }
            set
            {
                if (_isValid == value)
                    return;

                _isValid = value;
                NotifyOfPropertyChange(nameof(IsValid));
                NotifyOfPropertyChange(nameof(CanOk));
            }
        }

        public bool CanOk => !_hasPendingRequest && _isValid
            && _selectedAgent.Model.AccessManager.CanUploadPackage()
            && _selectedAgent.Model.AccessManager.CanAddBot()
            && _selectedAgent.Model.AccessManager.CanGetBotFolderInfo(BotFolderId.AlgoData)
            && _selectedAgent.Model.AccessManager.CanUploadBotFile();

        public bool HasError => !string.IsNullOrEmpty(_error);

        public string Error
        {
            get { return _error; }
            set
            {
                if (_error == value)
                    return;

                _error = value;
                NotifyOfPropertyChange(nameof(Error));
                NotifyOfPropertyChange(nameof(HasError));
            }
        }

        public ProgressViewModel CopyProgress { get; }


        public CopyBotInstanceViewModel(AlgoEnvironment algoEnv, string agentName, string botId)
        {
            _algoEnv = algoEnv;

            DisplayName = "Copy Bot Instance";

            Accounts = _algoEnv.Agents.SelectMany(a => a.Accounts).AsObservable();
            SelectedAccount = Accounts.FirstOrDefault();
            InstanceId = botId;
            From = $"{agentName}/{botId}";

            _fromBotId = botId;
            _fromAgent = _algoEnv.Agents.Snapshot.First(a => a.Name == agentName);
            _fromBotRemoved = !_fromAgent.Model.Bots.Snapshot.ContainsKey(_fromBotId);
            _fromAgent.Model.Bots.Updated += FromAgentOnBotsUpdated;

            CopyProgress = new ProgressViewModel();
        }


        public async void Ok()
        {
            HasPendingRequest = true;
            try
            {
                Error = null;

                _logger.Info($"Copying bot from '{From}' to '{_selectedAgent.Name}/{_selectedAccount.DisplayName}/{InstanceId}'");

                if (!_fromAgent.Model.Bots.Snapshot.TryGetValue(_fromBotId, out var srcBot))
                    throw new ArgumentException("Can't find bot to copy");

                var dstConfig = srcBot.Config.Clone();

                if (_fromAgent.Model.IsRemote || _selectedAgent.Model.IsRemote)
                {
                    await ResolvePackage(srcBot, dstConfig);
                }
                else
                {
                    _logger.Info("Both agents are local. No package resolving required");
                }

                dstConfig.InstanceId = InstanceId;
                await _selectedAgent.Model.AddBot(_selectedAccount.Key, dstConfig);
                _logger.Info($"Created bot {dstConfig.InstanceId} on {_selectedAgent.Name}");

                await ResolveBotFiles(srcBot, dstConfig);

                _selectedAgent.OpenBotState(dstConfig.InstanceId);

                TryClose();
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                _logger.Error(ex, "Failed to copy bot instance");
            }
            HasPendingRequest = false;
        }

        public void Cancel()
        {
            TryClose();
        }


        protected override void OnDeactivate(bool close)
        {
            DeinitAlgoAgent(_selectedAgent);
            _fromAgent.Model.Bots.Updated -= FromAgentOnBotsUpdated;

            base.OnDeactivate(close);
        }


        private void Validate()
        {
            IsValid = !_fromBotRemoved && SelectedAccount != null && IsInstanceIdValid;
        }

        private void InitAlgoAgent(AlgoAgentViewModel agent)
        {
            if (agent != null)
            {
                agent.Model.AccessLevelChanged += AgentOnAccessLevelChanged;
                agent.Model.Bots.Updated += AgentOnBotsUpdated;
            }
        }

        private void DeinitAlgoAgent(AlgoAgentViewModel agent)
        {
            if (agent != null)
            {
                agent.Model.AccessLevelChanged -= AgentOnAccessLevelChanged;
                agent.Model.Bots.Updated -= AgentOnBotsUpdated;
            }
        }

        private void AgentOnAccessLevelChanged()
        {
            NotifyOfPropertyChange(nameof(CanOk));
        }

        private void AgentOnBotsUpdated(DictionaryUpdateArgs<string, ITradeBot> args)
        {
            if (args.Action == DLinqAction.Insert || args.Action == DLinqAction.Remove)
            {
                Validate();
                NotifyOfPropertyChange(nameof(IsInstanceIdValid));
            }
        }

        private void FromAgentOnBotsUpdated(DictionaryUpdateArgs<string, ITradeBot> args)
        {
            if (args.Action == DLinqAction.Remove && args.Key == _fromBotId)
            {
                _fromBotRemoved = true;
                Error = "Can't copy bot that has been removed";
            }
            else if (_fromBotRemoved && args.Action == DLinqAction.Insert && args.Key == _fromBotId)
            {
                _fromBotRemoved = false;
                Error = null;
            }
        }

        private async Task ResolvePackage(ITradeBot srcBot, PluginConfig dstConfig)
        {
            if (!_fromAgent.Model.Packages.Snapshot.TryGetValue(srcBot.Config.Key.GetPackageKey(), out var srcPackage))
                throw new ArgumentException("Can't find bot package");

            var uploadSrcPackage = false;
            var dstPackageKey = dstConfig.Key.GetPackageKey();
            dstPackageKey.Location = RepositoryLocation.LocalRepository; //remote bot agents have only local package location
            var dstPackage = _selectedAgent.Model.Packages.Snapshot.Values.Where(p => p.Identity.Size == srcPackage.Identity.Size && p.Identity.Hash == srcPackage.Identity.Hash)
                .OrderBy(p => p.Key.Location == dstPackageKey.Location ? 0 : 1).ThenBy(p => p.Key.Name == dstPackageKey.Name ? 0 : 1).FirstOrDefault();
            if (dstPackage != null)
            {
                _logger.Info($"'{_selectedAgent.Name}' has matching package {dstPackage.Key.Name}({dstPackage.Key.Location})");
                _logger.Info($"Src package: {srcPackage.Identity.Hash}; Dst package: {dstPackage.Identity.Hash}");
                dstPackageKey = dstPackage.Key;
            }
            else
            {
                _logger.Info($"'{_selectedAgent.Name}' has no matching package.");
                uploadSrcPackage = true;
            }

            if (uploadSrcPackage)
            {
                var progressListener = new FileProgressListenerAdapter(CopyProgress, srcPackage.Identity.Size);

                var srcPath = "";
                if (!_fromAgent.Model.IsRemote)
                {
                    srcPath = srcPackage.Identity.FilePath;
                    _logger.Info($"Package is local. Using path: {srcPath}");
                }
                else
                {
                    srcPath = Path.GetTempFileName();
                    CopyProgress.SetMessage($"Downloading package {srcPackage.Key.Name} from {_fromAgent.Name}");
                    await _fromAgent.Model.DownloadPackage(srcPackage.Key, srcPath, progressListener);
                    _logger.Info($"Downloaded remote package to: {srcPath}");
                }
                if (_selectedAgent.Model.Packages.Snapshot.ContainsKey(dstPackageKey))
                {
                    var dstPackageName = Path.GetFileNameWithoutExtension(dstPackageKey.Name);
                    for (var i = 1; _selectedAgent.Model.Packages.Snapshot.ContainsKey(dstPackageKey); i++)
                    {
                        dstPackageKey.Name = $"{dstPackageName} ({i}).ttalgo";
                    }
                }
                CopyProgress.SetMessage($"Uploading package {dstPackageKey.Name} to {_selectedAgent.Name}");
                await _selectedAgent.Model.UploadPackage(dstPackageKey.Name, srcPath, progressListener);
                _logger.Info($"Uploaded remote package to as {dstPackageKey.Name} to {_selectedAgent.Name}");
                if (_fromAgent.Model.IsRemote)
                    File.Delete(srcPath);

                CopyProgress.StartProgress(0, 100);
                for (var i = 0; i < 10; i++) // give selected agent some time to parse package so bot is not created in broken state
                {
                    CopyProgress.SetProgress(10 * i);
                    await Task.Delay(100);
                }
            }

            dstConfig.Key.PackageName = dstPackageKey.Name;
            dstConfig.Key.PackageLocation = dstPackageKey.Location;
        }

        private async Task ResolveBotFiles(ITradeBot srcBot, PluginConfig dstConfig)
        {
            var srcAlgoDataDir = await _fromAgent.Model.GetBotFolderInfo(srcBot.InstanceId, BotFolderId.AlgoData);
            var dstAlgoDataDir = await _selectedAgent.Model.GetBotFolderInfo(dstConfig.InstanceId, BotFolderId.AlgoData);
            foreach (FileParameter fileParam in dstConfig.Properties.Where(p => p is FileParameter))
            {
                var fileName = Path.GetFileName(fileParam.FileName);
                var srcPath = "";
                if (!_fromAgent.Model.IsRemote)
                {
                    srcPath = fileParam.FileName;
                    if (Path.GetFullPath(srcPath) != srcPath)
                    {
                        srcPath = Path.Combine(srcAlgoDataDir.Path, fileParam.FileName);
                    }
                    _logger.Info($"Bot file {fileName} is local. Using path: {srcPath}");
                }
                else
                {
                    srcPath = Path.GetTempFileName();
                    CopyProgress.SetMessage($"Downloading bot file {fileName} from {_fromAgent.Name}");
                    var progressListener = new FileProgressListenerAdapter(CopyProgress, srcAlgoDataDir.Files.First(f => f.Name == fileName).Size);
                    await _fromAgent.Model.DownloadBotFile(srcBot.InstanceId, BotFolderId.AlgoData, fileName, srcPath, progressListener);
                    _logger.Info($"Downloaded bot file to: {srcPath}");
                }

                if (!_selectedAgent.Model.IsRemote)
                {
                    if (_fromAgent.Model.IsRemote || (!_fromAgent.Model.IsRemote && srcPath.StartsWith(srcAlgoDataDir.Path)))
                    {
                        if (!Directory.Exists(dstAlgoDataDir.Path))
                            Directory.CreateDirectory(dstAlgoDataDir.Path);
                        var dstPath = Path.Combine(dstAlgoDataDir.Path, fileName);
                        File.Copy(srcPath, dstPath, true);
                        _logger.Info($"Bot file {fileName} copied to: {dstPath}");
                    }
                    else
                    {
                        _logger.Info($"Bot file {fileName} uses original path: {srcPath}");
                    }
                }
                else
                {
                    var fileInfo = new FileInfo(srcPath);
                    CopyProgress.SetMessage($"Uploading bot file {fileName} to {_selectedAgent.Model.Name}");
                    var progressListener = new FileProgressListenerAdapter(CopyProgress, fileInfo.Length);
                    await _selectedAgent.Model.UploadBotFile(dstConfig.InstanceId, BotFolderId.AlgoData, fileName, srcPath, progressListener);
                    _logger.Info($"Downloaded bot file {fileName} to {_selectedAgent.Model.Name}");
                }

                if (_fromAgent.Model.IsRemote)
                    File.Delete(srcPath);
            }
        }
    }
}