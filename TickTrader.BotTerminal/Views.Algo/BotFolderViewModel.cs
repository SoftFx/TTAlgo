using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using TickTrader.Algo.Common.Info;

namespace TickTrader.BotTerminal
{
    internal class BotFolderViewModel : Screen, IWindowModel
    {
        private readonly static ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        private AlgoEnvironment _algoEnv;
        private AlgoAgentViewModel _selectedAgent;
        private AlgoBotViewModel _selectedBot;
        private BotFolderId _selectedFolderId;
        private bool _isEnabled;
        private string _path;
        private VarList<BotFileViewModel> _botFiles;
        private BotFileViewModel _selectedBotFile;
        private bool _isDownloading;
        private bool _isUploading;
        private string _toFileName;
        private string _fromPath;
        private string _toPath;
        private bool _loading;
        private string _error;


        public IObservableList<AlgoAgentViewModel> Agents { get; }

        public AlgoAgentViewModel SelectedAgent
        {
            get { return _selectedAgent; }
            set
            {
                if (_selectedAgent == value)
                    return;

                DeinitAlgoAgent(_selectedAgent);
                _selectedAgent = value;
                InitAlgoAgent(_selectedAgent);
                NotifyOfPropertyChange(nameof(SelectedAgent));
                NotifyOfPropertyChange(nameof(Bots));
            }
        }

        public IObservableList<AlgoBotViewModel> Bots { get; private set; }

        public AlgoBotViewModel SelectedBot
        {
            get { return _selectedBot; }
            set
            {
                if (_selectedBot == value)
                    return;

                _selectedBot = value;
                NotifyOfPropertyChange(nameof(SelectedBot));
                NotifyOfPropertyChange(nameof(CanUploadFile));
                NotifyOfPropertyChange(nameof(CanRefreshFolderInfo));
                NotifyOfPropertyChange(nameof(CanClear));
                RefreshBotFiles();
            }
        }

        public BotFolderId[] AvailableFolderIds { get; }

        public BotFolderId SelectedFolderId
        {
            get { return _selectedFolderId; }
            set
            {
                if (_selectedFolderId == value)
                    return;

                _selectedFolderId = value;
                NotifyOfPropertyChange(nameof(SelectedFolderId));
                NotifyOfPropertyChange(nameof(CanUploadFile));
                RefreshBotFiles();
            }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled == value)
                    return;

                _isEnabled = value;
                NotifyOfPropertyChange(nameof(IsEnabled));
            }
        }

        public string Path
        {
            get { return _path; }
            private set
            {
                if (_path == value)
                    return;

                _path = value;
                NotifyOfPropertyChange(nameof(Path));
            }
        }

        public ICollectionView BotFiles { get; }

        public BotFileViewModel SelectedBotFile
        {
            get { return _selectedBotFile; }
            set
            {
                if (_selectedBotFile == value)
                    return;

                _selectedBotFile = value;
                NotifyOfPropertyChange(nameof(SelectedBotFile));
                NotifyOfPropertyChange(nameof(CanDownloadFile));
                NotifyOfPropertyChange(nameof(CanDeleteFile));
            }
        }

        public bool CanUploadFile => SelectedBot != null && SelectedFolderId == BotFolderId.AlgoData;

        public bool CanDownloadFile => SelectedBotFile != null;

        public bool CanDeleteFile => SelectedBotFile != null;

        public bool CanRefreshFolderInfo => SelectedBot != null;

        public bool CanClear => SelectedBot != null;

        public bool IsDownloading
        {
            get { return _isDownloading; }
            set
            {
                if (_isDownloading == value)
                    return;

                _isDownloading = value;
                _isUploading = false;
                Error = null;
                NotifyOfPropertyChange(nameof(IsDownloading));
                NotifyOfPropertyChange(nameof(IsUploading));
            }
        }

        public bool IsUploading
        {
            get { return _isUploading; }
            set
            {
                if (_isUploading == value)
                    return;

                _isUploading = value;
                _isDownloading = false;
                Error = null;
                NotifyOfPropertyChange(nameof(IsUploading));
                NotifyOfPropertyChange(nameof(IsDownloading));
            }
        }

        public string ToFileName
        {
            get { return _toFileName; }
            set
            {
                if (_toFileName == value)
                    return;

                _toFileName = value;
                NotifyOfPropertyChange(nameof(ToFileName));
                Validate();
            }
        }

        public string FromPath
        {
            get { return _fromPath; }
            set
            {
                if (_fromPath == value)
                    return;

                _fromPath = value;
                NotifyOfPropertyChange(nameof(FromPath));
                if (IsUploading)
                {
                    ToFileName = System.IO.Path.GetFileName(FromPath);
                }
            }
        }

        public string ToPath
        {
            get { return _toPath; }
            set
            {
                if (_toPath == value)
                    return;

                _toPath = value;
                NotifyOfPropertyChange(nameof(ToPath));
            }
        }

        public bool Loading
        {
            get { return _loading; }
            set
            {
                if (_loading == value)
                    return;

                _loading = value;
                NotifyOfPropertyChange(nameof(Loading));
            }
        }

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

        public bool HasError => !string.IsNullOrWhiteSpace(Error);


        private BotFolderViewModel(AlgoEnvironment algoEnv)
        {
            _algoEnv = algoEnv;

            DisplayName = "Manage Bot Files";

            Agents = _algoEnv.BotAgents.Select(b => b.Agent).AsObservable();
            _botFiles = new VarList<BotFileViewModel>();
            BotFiles = CollectionViewSource.GetDefaultView(_botFiles.AsObservable());
            BotFiles.SortDescriptions.Add(new SortDescription { PropertyName = "Name", Direction = ListSortDirection.Ascending });

            AvailableFolderIds = Enum.GetValues(typeof(BotFolderId)).Cast<BotFolderId>().ToArray();
            IsEnabled = true;
        }

        public BotFolderViewModel(AlgoEnvironment algoEnv, string agentName)
            : this(algoEnv)
        {
            SelectedAgent = Agents.FirstOrDefault(a => a.Name == agentName);
        }

        public BotFolderViewModel(AlgoEnvironment algoEnv, string agentName, string botId, BotFolderId folderId)
            : this(algoEnv, agentName)
        {
            _selectedFolderId = folderId;
            SelectedBot = Bots.FirstOrDefault(b => b.InstanceId == botId);
        }


        public void UploadFile()
        {
            FromPath = null;
            ToPath = $"{SelectedAgent.Name}/{SelectedFolderId}/{SelectedBot.InstanceId}/";
            ToFileName = null;
            IsUploading = true;
        }

        public void DownloadFile()
        {
            FromPath = $"{SelectedAgent.Name}/{SelectedFolderId}/{SelectedBot.InstanceId}/{SelectedBotFile.Name}";
            ToPath = SelectedBotFile.Name;
            IsDownloading = true;
        }

        public async void DeleteFile()
        {
            if (SelectedAgent == null || SelectedBot == null)
                return;

            IsEnabled = false;

            try
            {
                await SelectedAgent.Model.ClearBotFolder(SelectedBot.InstanceId, SelectedFolderId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to delete bot file");
            }
            RefreshBotFilesInternal();

            IsEnabled = true;
        }

        public void RefreshFolderInfo()
        {
            RefreshBotFiles();
        }

        public async void Clear()
        {
            if (SelectedAgent == null || SelectedBot == null)
                return;

            IsEnabled = false;

            try
            {
                await SelectedAgent.Model.ClearBotFolder(SelectedBot.InstanceId, SelectedFolderId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to clear bot files");
            }
            RefreshBotFilesInternal();

            IsEnabled = true;
        }

        public async void StartLoading()
        {
            if (SelectedAgent == null || SelectedBot == null)
                return;

            IsEnabled = false;

            Loading = true;
            if (IsDownloading && SelectedBotFile != null)
            {
                try
                {
                    await SelectedAgent.Model.DownloadBotFile(SelectedBot.InstanceId, SelectedFolderId, SelectedBotFile.Name, ToPath);
                }
                catch(Exception ex)
                {
                    _logger.Error(ex, "Failed to download bot file");
                    Error = ex.Message;
                }
            }
            else if (IsUploading)
            {
                try
                {
                    await SelectedAgent.Model.UploadBotFile(SelectedBot.InstanceId, SelectedFolderId, ToFileName, FromPath);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to upload bot file");
                    Error = ex.Message;
                }
            }
            Loading = false;

            RefreshBotFilesInternal();

            IsEnabled = true;
        }


        private void InitAlgoAgent(AlgoAgentViewModel agent)
        {
            if (agent != null)
            {
                Bots = agent.Bots.AsObservable();
                SelectedBot = null;

                agent.Model.Bots.Updated += BotAgentBotsUpdated;
            }
        }

        private void DeinitAlgoAgent(AlgoAgentViewModel agent)
        {
            if (agent != null)
            {
                agent.Model.Bots.Updated -= BotAgentBotsUpdated;
            }
        }

        private void BotAgentBotsUpdated(DictionaryUpdateArgs<string, ITradeBot> args)
        {
            if (args.Key == SelectedBot?.InstanceId && args.Action == DLinqAction.Remove)
            {
                SelectedBot = null;
            }
        }

        private void RefreshBotFiles()
        {
            if (!IsEnabled)
                return;

            IsEnabled = false;

            RefreshBotFilesInternal();
            Validate();

            IsEnabled = true;
        }

        private async void RefreshBotFilesInternal()
        {
            Path = null;
            _botFiles.Clear();

            if (SelectedAgent == null || SelectedBot == null)
                return;

            try
            {
                var folderInfo = await SelectedAgent.Model.GetBotFolderInfo(SelectedBot.InstanceId, SelectedFolderId);
                Path = folderInfo.Path;
                foreach (var botFile in folderInfo.Files)
                {
                    _botFiles.Add(new BotFileViewModel(botFile, this));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to refresh bot files");
            }
        }

        private void Validate()
        {
            if (_botFiles.Values.Any(f => f.Name == ToFileName))
                Error = "File will be overwritten";
            else Error = null;
        }
    }


    internal class BotFileViewModel
    {
        private BotFolderViewModel _botFolder;


        public BotFileInfo Info { get; }

        public string Name => Info.Name;

        public long Size => Info.Size;

        public string SizeText => $"{Info.Size / 1024} KB";


        public BotFileViewModel(BotFileInfo info, BotFolderViewModel botFolder)
        {
            Info = info;
            _botFolder = botFolder;
        }
    }
}
