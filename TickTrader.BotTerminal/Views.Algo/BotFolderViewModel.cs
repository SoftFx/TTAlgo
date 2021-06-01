using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using TickTrader.Algo.Domain;
using TickTrader.Algo.ServerControl;

namespace TickTrader.BotTerminal
{
    internal class BotFolderViewModel : Screen, IWindowModel, IFileProgressListener
    {
        private readonly static ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        private AlgoEnvironment _algoEnv;
        private AlgoAgentViewModel _selectedAgent;
        private AlgoBotViewModel _selectedBot;
        private PluginFolderInfo.Types.PluginFolderId _selectedFolderId;
        private bool _isEnabled;
        private string _path;
        private VarList<BotFileViewModel> _botFiles;
        private BotFileViewModel _selectedBotFile;
        private bool _isDownloading;
        private bool _isUploading;
        private string _toFileName;
        private string _fromPath;
        private string _toPath;
        private string _error;
        private double _progressValue;
        private long _currentProgress;
        private long _fullProgress;


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
                NotifyOfPropertyChange(nameof(CanStartLoading));
                NotifyOfPropertyChange(nameof(CanRefreshFolderInfo));
                NotifyOfPropertyChange(nameof(CanClear));
                RefreshBotFiles();
            }
        }

        public PluginFolderInfo.Types.PluginFolderId[] AvailableFolderIds { get; }

        public PluginFolderInfo.Types.PluginFolderId SelectedFolderId
        {
            get { return _selectedFolderId; }
            set
            {
                if (_selectedFolderId == value)
                    return;

                _selectedFolderId = value;
                NotifyOfPropertyChange(nameof(SelectedFolderId));
                NotifyOfPropertyChange(nameof(CanUploadFile));
                NotifyOfPropertyChange(nameof(CanStartLoading));
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
                NotifyOfPropertyChange(nameof(CanStartLoading));
                NotifyOfPropertyChange(nameof(CanDeleteFile));
                UpdateLoadingPaths();
            }
        }

        public bool CanUploadFile => SelectedBot != null && SelectedFolderId == PluginFolderInfo.Types.PluginFolderId.AlgoData && SelectedAgent.Model.AccessManager.CanUploadBotFile();

        public bool CanDownloadFile => SelectedBotFile != null && SelectedAgent.Model.AccessManager.CanDownloadBotFile(SelectedFolderId);

        public bool CanDeleteFile => SelectedBotFile != null && SelectedAgent.Model.AccessManager.CanDeleteBotFile();

        public bool CanRefreshFolderInfo => SelectedBot != null && SelectedAgent.Model.AccessManager.CanGetBotFolderInfo(SelectedFolderId);

        public bool CanClear => SelectedBot != null && SelectedAgent.Model.AccessManager.CanClearBotFolder();

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
                NotifyOfPropertyChange(nameof(CanStartLoading));
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
                NotifyOfPropertyChange(nameof(CanStartLoading));
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
                DropProgress();
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
                DropProgress();
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
                DropProgress();
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

        public bool CanStartLoading => (IsUploading && CanUploadFile) || (IsDownloading && CanDownloadFile);

        public double ProgressValue
        {
            get { return _progressValue; }
            set
            {
                if (_progressValue == value)
                    return;

                _progressValue = value;
                NotifyOfPropertyChange(nameof(ProgressValue));
            }
        }


        private BotFolderViewModel(AlgoEnvironment algoEnv)
        {
            _algoEnv = algoEnv;

            DisplayName = "Manage Bot Files";

            Agents = _algoEnv.BotAgents.Select(b => b.Agent).AsObservable();
            _botFiles = new VarList<BotFileViewModel>();
            BotFiles = CollectionViewSource.GetDefaultView(_botFiles.AsObservable());
            BotFiles.SortDescriptions.Add(new SortDescription { PropertyName = "Name", Direction = ListSortDirection.Ascending });

            AvailableFolderIds = Enum.GetValues(typeof(PluginFolderInfo.Types.PluginFolderId)).Cast<PluginFolderInfo.Types.PluginFolderId>().ToArray();
            SelectedFolderId = PluginFolderInfo.Types.PluginFolderId.BotLogs;
            IsEnabled = true;
        }

        public BotFolderViewModel(AlgoEnvironment algoEnv, string agentName)
            : this(algoEnv)
        {
            SelectedAgent = Agents.FirstOrDefault(a => a.Name == agentName);
        }

        public BotFolderViewModel(AlgoEnvironment algoEnv, string agentName, string botId, PluginFolderInfo.Types.PluginFolderId folderId)
            : this(algoEnv, agentName)
        {
            _selectedFolderId = folderId;
            SelectedBot = Bots.FirstOrDefault(b => b.InstanceId == botId);
        }


        public void UploadFile()
        {
            FromPath = null;
            ToPath = $"{SelectedAgent.Name}/{SelectedFolderId}/{SelectedBot.InstanceId}/";
            ToFileName = SelectedBotFile?.Name;
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
            if (SelectedAgent == null || SelectedBot == null || SelectedBotFile == null)
                return;

            IsEnabled = false;

            try
            {
                await SelectedAgent.Model.DeleteBotFile(SelectedBot.InstanceId, SelectedFolderId, SelectedBotFile.Name);
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

            if (IsDownloading && SelectedBotFile != null)
            {
                try
                {
                    _fullProgress = SelectedBotFile.Size;
                    await SelectedAgent.Model.DownloadBotFile(SelectedBot.InstanceId, SelectedFolderId, SelectedBotFile.Name, ToPath, this);
                    RefreshBotFilesInternal();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to download bot file");
                    Error = ex.Message;
                }
            }
            else if (IsUploading)
            {
                try
                {
                    _fullProgress = new FileInfo(FromPath).Length;
                    await SelectedAgent.Model.UploadBotFile(SelectedBot.InstanceId, SelectedFolderId, ToFileName, FromPath, this);
                    RefreshBotFilesInternal();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to upload bot file");
                    Error = ex.Message;
                }
            }

            IsEnabled = true;
        }


        private void InitAlgoAgent(AlgoAgentViewModel agent)
        {
            if (agent != null)
            {
                Bots = agent.Bots.AsObservable();
                SelectedBot = null;

                agent.Model.Bots.Updated += BotAgentBotsUpdated;
                agent.Model.AccessLevelChanged += OnAccessLevelChanged;
            }
        }

        private void DeinitAlgoAgent(AlgoAgentViewModel agent)
        {
            if (agent != null)
            {
                agent.Model.Bots.Updated -= BotAgentBotsUpdated;
                agent.Model.AccessLevelChanged -= OnAccessLevelChanged;
            }
        }

        private void BotAgentBotsUpdated(DictionaryUpdateArgs<string, ITradeBot> args)
        {
            if (args.Key == SelectedBot?.InstanceId && args.Action == DLinqAction.Remove)
            {
                SelectedBot = null;
            }
        }

        private void OnAccessLevelChanged()
        {
            NotifyOfPropertyChange(nameof(CanUploadFile));
            NotifyOfPropertyChange(nameof(CanDownloadFile));
            NotifyOfPropertyChange(nameof(CanStartLoading));
            NotifyOfPropertyChange(nameof(CanDeleteFile));
            NotifyOfPropertyChange(nameof(CanRefreshFolderInfo));
            NotifyOfPropertyChange(nameof(CanClear));
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
            var currentFile = SelectedBotFile?.Name;

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

            SelectedBotFile = _botFiles.Values.FirstOrDefault(f => f.Name == currentFile);
        }

        private void Validate()
        {
            if (_botFiles.Values.Any(f => f.Name == ToFileName))
                Error = "File will be overwritten";
            else Error = null;
        }

        private void UpdateLoadingPaths()
        {
            if (SelectedAgent == null || SelectedBot == null)
                return;

            if (IsUploading)
            {
                ToPath = $"{SelectedAgent.Name}/{SelectedFolderId}/{SelectedBot.InstanceId}/";
                ToFileName = SelectedBotFile?.Name;
            }
            else if (IsDownloading && SelectedBotFile != null)
            {
                FromPath = $"{SelectedAgent.Name}/{SelectedFolderId}/{SelectedBot.InstanceId}/{SelectedBotFile.Name}";
                ToPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(ToPath), SelectedBotFile.Name);
            }
        }

        private void DropProgress()
        {
            if (SelectedBotFile != null)
                ProgressValue = 0;
        }


        #region IFileProgressListener implementation

        void IFileProgressListener.Init(long initialProgress)
        {
            _currentProgress = initialProgress;
            ProgressValue = 100.0 * _currentProgress / _fullProgress;
        }

        void IFileProgressListener.IncrementProgress(long progressValue)
        {
            _currentProgress += progressValue;
            ProgressValue = 100.0 * _currentProgress / _fullProgress;
        }

        #endregion IFileProgressListener implementation
    }


    internal class BotFileViewModel
    {
        private BotFolderViewModel _botFolder;


        public PluginFileInfo Info { get; }

        public string Name => Info.Name;

        public long Size => Info.Size;

        public string SizeText => MakeSizeText(Info.Size);


        public BotFileViewModel(PluginFileInfo info, BotFolderViewModel botFolder)
        {
            Info = info;
            _botFolder = botFolder;
        }


        private string MakeSizeText(long size)
        {
            if (size < 1024)
            {
                return $"{size} bytes";
            }
            if (size < 1024 * 1024)
            {
                return $"{size / 1024.0:F1} KB";
            }
            if (size < 1024 * 1024 * 1024)
            {
                return $"{size / 1024.0 / 1024.0:F1} MB";
            }
            return $"{size / 1024.0 / 1024.0 / 1024.0:F1} GB";
        }
    }
}
