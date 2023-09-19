using Caliburn.Micro;
using Machinarium.Var;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.AppCommon;
using TickTrader.Algo.AppCommon.Update;
using TickTrader.Algo.AutoUpdate;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.WpfWindowsSupportLibrary;

namespace TickTrader.BotTerminal
{
    internal sealed class AutoUpdateViewModel : Screen, IWindowModel
    {
        private enum ViewPage { UpdateList, UpdateProcess };


        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<AutoUpdateViewModel>();
        private static readonly UpdateAssetTypes[] _terminalAssets = new[] { UpdateAssetTypes.TerminalUpdate, UpdateAssetTypes.Setup };
        private static readonly UpdateAssetTypes[] _serverAssets = new[] { UpdateAssetTypes.ServerUpdate, UpdateAssetTypes.Setup };

        private readonly VarContext _context = new();
        private readonly AutoUpdateService _updateSvc;
        private readonly AlgoAgentViewModel _remoteAgent;
        private readonly bool _isRemoteUpdate;

        private ViewPage? _displayPage;


        public BoolProperty ShowUpdateListPage { get; }

        public BoolProperty ShowUpdateProcessPage { get; }

        public StrProperty CurrentVersion { get; }

        public BoolProperty UpdateListGuiEnabled { get; }

        public BoolProperty UpdatesLoaded { get; }

        public ObservableCollection<AppUpdateViewModel> AvailableUpdates { get; } = new();

        public Property<AppUpdateViewModel> SelectedUpdate { get; }

        public BoolProperty HasSelectedUpdate { get; }

        public StrProperty Status { get; }

        public BoolProperty StatusHasError { get; }

        public BoolProperty DownloadInProgress { get; }

        public StrProperty UpdateStatus { get; }

        public BoolProperty UpdateStatusHasError { get; }

        public StrProperty UpdateLog { get; }

        public BoolProperty UpdateInProgress { get; }

        public bool CanControlUpdate => !_isRemoteUpdate
            || (_remoteAgent.Model.VersionSpec.SupportsAutoUpdate && _remoteAgent.Model.AccessManager.CanControlServerUpdate());

        public bool CanInstallUpdate => CanControlUpdate && (SelectedUpdate.Value?.HasUpdate ?? false)
            && (SelectedUpdate.Value?.SupportedByMinVersion ?? false);

        public bool CanDownloadSetup => SelectedUpdate.Value?.HasSetup ?? false;

        public bool CanDiscardUpdateResult => CanControlUpdate && (_remoteAgent?.Model.UpdateSvcInfo.Status != AutoUpdateEnums.Types.ServiceStatus.Updating);


        private AutoUpdateViewModel()
        {
            ShowUpdateListPage = _context.AddBoolProperty();
            ShowUpdateProcessPage = _context.AddBoolProperty();
            CurrentVersion = _context.AddStrProperty();
            UpdateListGuiEnabled = _context.AddBoolProperty();
            UpdatesLoaded = _context.AddBoolProperty();
            HasSelectedUpdate = _context.AddBoolProperty();
            SelectedUpdate = _context.AddProperty<AppUpdateViewModel>().AddPostTrigger(_ => OnSelectedUpdateChanged());
            Status = _context.AddStrProperty();
            StatusHasError = _context.AddBoolProperty();
            DownloadInProgress = _context.AddBoolProperty();
            UpdateStatus = _context.AddStrProperty();
            UpdateStatusHasError = _context.AddBoolProperty();
            UpdateLog = _context.AddStrProperty();
            UpdateInProgress = _context.AddBoolProperty();
        }

        public AutoUpdateViewModel(AutoUpdateService updateSvc) : this()
        {
            _updateSvc = updateSvc;

            DisplayName = $"Auto Update - {EnvService.Instance.ApplicationName}";
            _isRemoteUpdate = false;
            var appVersion = AppVersionInfo.Current;
            CurrentVersion.Value = $"{appVersion.Version} ({appVersion.BuildDate})";

            _updateSvc.SetUpdateStateChangedCallback(OnUpdateStateChanged, false);
            OnUpdateStateChanged(); // initial load
        }

        public AutoUpdateViewModel(AutoUpdateService updateSvc, BotAgentViewModel remoteAgent) : this()
        {
            _updateSvc = updateSvc;
            _remoteAgent = remoteAgent.Agent;

            DisplayName = $"Auto Update - AlgoServer '{remoteAgent.Agent.Name}'";
            _isRemoteUpdate = true;
            _remoteAgent.Model.AccessLevelChanged += OnAgentAccessLevelChanged;
            _remoteAgent.Model.SnapshotLoaded += OnAgentSnapshotLoaded;
            _remoteAgent.Model.UpdateServiceStateChanged += OnAgentUpdateServiceStateChanged;

            InitRemoteAgentData();
        }


        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            if (close)
            {
                if (_remoteAgent != null)
                {
                    _remoteAgent.Model.AccessLevelChanged -= OnAgentAccessLevelChanged;
                    _remoteAgent.Model.SnapshotLoaded -= OnAgentSnapshotLoaded;
                    _remoteAgent.Model.UpdateServiceStateChanged -= OnAgentUpdateServiceStateChanged;
                }
                else
                {
                    _updateSvc.SetUpdateStateChangedCallback(null, false);
                }
            }

            return Task.CompletedTask;
        }


        public void RefreshUpdates()
        {
            _ = LoadUpdatesAsync(true);
        }

        public void OpenGithubRepo()
        {
            try
            {
                var sInfo = new ProcessStartInfo(AutoUpdateService.MainGithubRepo)
                {
                    UseShellExecute = true,
                };

                Process.Start(sInfo);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to open github link");
                MessageBoxManager.OkError(ex.Message);
            }
        }

        public void InstallUpdate()
        {
            _ = InstallUpdateAsync();
        }

        public void DownloadSetup()
        {
            _ = DownloadSetupAsync(SelectedUpdate.Value);
        }

        public void DiscardUpdateResult()
        {
            _ = DiscardUpdateResultAsync();
        }


        private void SetDisplayPage(ViewPage newPage)
        {
            if (_displayPage == newPage)
                return;

            _displayPage = newPage;
            ShowUpdateListPage.Value = newPage == ViewPage.UpdateList;
            ShowUpdateProcessPage.Value = newPage == ViewPage.UpdateProcess;

            InitDisplayPage();
        }

        private void InitDisplayPage()
        {
            var page = _displayPage;
            if (page == ViewPage.UpdateList)
            {
                _ = LoadUpdatesAsync();
            }
        }

        private void OnSelectedUpdateChanged()
        {
            if (SelectedUpdate == null)
                return;

            HasSelectedUpdate.Value = SelectedUpdate.Value != null;
            NotifyOfPropertyChange(nameof(CanInstallUpdate));
            NotifyOfPropertyChange(nameof(CanDownloadSetup));
        }

        private void OnAgentAccessLevelChanged()
        {
            NotifyOfPropertyChange(nameof(CanInstallUpdate));
            NotifyOfPropertyChange(nameof(CanDownloadSetup));
            NotifyOfPropertyChange(nameof(CanDiscardUpdateResult));
        }

        private void OnAgentSnapshotLoaded()
        {
            InitRemoteAgentData();
            InitDisplayPage();
        }

        private void InitRemoteAgentData()
        {
            if (!_remoteAgent.Model.VersionSpec.SupportsAutoUpdate)
            {
                CurrentVersion.Value = "AutoUpdate not supported";
                SetDisplayPage(ViewPage.UpdateList);
            }
            else
            {
                var versionInfo = _remoteAgent.Model.CurrentVersion;
                CurrentVersion.Value = $"{versionInfo.Version} ({versionInfo.ReleaseDate})";
                OnUpdateSvcInfoChanged(_remoteAgent.Model.UpdateSvcInfo);
            }
        }

        private void OnUpdateStateChanged()
        {
            var updInfo = new UpdateServiceInfo();
            if (_updateSvc.HasPendingUpdate)
            {
                updInfo.Status = _updateSvc.UpdateStatus switch
                {
                    UpdateStatusCodes.Pending => AutoUpdateEnums.Types.ServiceStatus.Updating,
                    UpdateStatusCodes.Completed => AutoUpdateEnums.Types.ServiceStatus.UpdateSuccess,
                    _ => AutoUpdateEnums.Types.ServiceStatus.UpdateFailed,
                };
                updInfo.StatusDetails = _updateSvc.UpdateStatusDetails;
                updInfo.UpdateLog = _updateSvc.UpdateLog;
            }
            else
            {
                updInfo.Status = AutoUpdateEnums.Types.ServiceStatus.Idle;
                updInfo.StatusDetails = null;
                updInfo.UpdateLog = null;
            }

            OnUpdateSvcInfoChanged(updInfo);
        }

        private void OnAgentUpdateServiceStateChanged() => OnUpdateSvcInfoChanged(_remoteAgent.Model.UpdateSvcInfo);

        private void OnUpdateSvcInfoChanged(UpdateServiceInfo updateSvcInfo)
        {
            var status = updateSvcInfo.Status;
            switch (status)
            {
                case AutoUpdateEnums.Types.ServiceStatus.Updating:
                case AutoUpdateEnums.Types.ServiceStatus.UpdateSuccess:
                case AutoUpdateEnums.Types.ServiceStatus.UpdateFailed:
                    SetDisplayPage(ViewPage.UpdateProcess);
                    break;
                case AutoUpdateEnums.Types.ServiceStatus.Idle:
                    SetDisplayPage(ViewPage.UpdateList);
                    break;
            }
            UpdateStatus.Value = updateSvcInfo.StatusDetails;
            UpdateStatusHasError.Value = status == AutoUpdateEnums.Types.ServiceStatus.UpdateFailed;
            UpdateLog.Value = updateSvcInfo.UpdateLog;
            UpdateInProgress.Value = status == AutoUpdateEnums.Types.ServiceStatus.Updating;
            NotifyOfPropertyChange(nameof(CanDiscardUpdateResult));
        }

        private async Task LoadUpdatesAsync(bool forced = false)
        {
            UpdateListGuiEnabled.Value = false;
            UpdatesLoaded.Value = false;
            Status.Value = string.Empty;
            StatusHasError.Value = false;

            try
            {
                if (_isRemoteUpdate)
                    await LoadRemoteUpdatesInternal(forced);
                else
                    await LoadLocalUpdatesInternal(forced);
            }
            catch (Exception ex)
            {
                var errMsg = _isRemoteUpdate
                    ? $"Failed to load updates for server {_remoteAgent?.Name}"
                    : "Failed to load updates";
                _logger.Error(ex, errMsg);
            }

            UpdatesLoaded.Value = true;
            UpdateListGuiEnabled.Value = true;
        }

        private async Task LoadLocalUpdatesInternal(bool forced)
        {
            var updateList = await _updateSvc.GetUpdates(forced);

            var currentVersion = AppVersionInfo.Current.Version;
            AvailableUpdates.Clear();
            foreach (var update in updateList.Updates)
                if (update.AvailableAssets.Any(a => _terminalAssets.Contains(a)))
                    AvailableUpdates.Add(new AppUpdateViewModel(update, currentVersion));

            if (updateList.Errors.Count > 0)
            {
                Status.Value = string.Join(Environment.NewLine, updateList.Errors);
                StatusHasError.Value = true;
            }
        }

        private async Task LoadRemoteUpdatesInternal(bool forced)
        {
            var agent = _remoteAgent;
            ServerUpdateList serverUpdList = null;
            string currentVersion = null;
            if (agent.Model.VersionSpec.SupportsAutoUpdate)
            {
                serverUpdList = await agent.Model.GetServerUpdateList(forced);
                currentVersion = agent.Model.CurrentVersion.Version;
            }

            var localUpdList = await _updateSvc.GetUpdates(forced);
            AvailableUpdates.Clear();

            if (serverUpdList != null)
            {
                // display updates available on server side
                foreach (var serverUpd in serverUpdList.Updates)
                {
                    // Match releases from main repo to use server-side update download and allow downloading setup
                    var update = localUpdList.Updates.FirstOrDefault(u => u.SrcId == AutoUpdateService.MainSourceName && u.VersionId == serverUpd.ReleaseId);
                    AvailableUpdates.Add(new AppUpdateViewModel(update, serverUpd, currentVersion));
                }
            }

            foreach (var update in localUpdList.Updates)
                if (update.AvailableAssets.Any(a => _serverAssets.Contains(a)))
                {
                    var hasServerUpd = serverUpdList != null && update.SrcId == AutoUpdateService.MainSourceName
                        && serverUpdList.Updates.Any(u => u.ReleaseId == update.VersionId);

                    // Skip duplicate updates
                    if (!hasServerUpd)
                        AvailableUpdates.Add(new AppUpdateViewModel(update, null, currentVersion));
                }

            var errors = serverUpdList != null ? Enumerable.Concat(localUpdList.Errors, serverUpdList.Errors) : localUpdList.Errors;
            if (errors.Any())
            {
                Status.Value = string.Join(Environment.NewLine, errors.Distinct(LoadUpdatesErrorEquailityComparer.Instance));
                StatusHasError.Value = true;
            }
        }

        private async Task InstallUpdateAsync()
        {
            UpdateListGuiEnabled.Value = false;
            Status.Value = string.Empty;
            StatusHasError.Value = false;
            var failed = false;

            try
            {
                var update = SelectedUpdate.Value;
                if (_isRemoteUpdate)
                    await InstallRemoteUpdateInternal(update);
                else
                    await InstallLocalUpdateInternal(update);
            }
            catch (Exception ex)
            {
                var errMsg = _isRemoteUpdate
                    ? $"Failed to install update for server {_remoteAgent?.Name}"
                    : "Failed to install update";
                _logger.Error(ex, errMsg);
                Status.Value = "Update failed unexpectedly. See logs...";
                StatusHasError.Value = true;
                failed = true;
            }

            if (failed)
            {
                UpdateListGuiEnabled.Value = true;
            }
        }

        private async Task InstallLocalUpdateInternal(AppUpdateViewModel update)
        {
            await _updateSvc.InstallUpdate(update.Source, update.VersionId);
        }

        private async Task InstallRemoteUpdateInternal(AppUpdateViewModel update)
        {
            var agent = _remoteAgent;
            var res = default(StartServerUpdateResponse);
            if (update.IsRemoteUpdate)
            {
                res = await agent.Model.StartServerUpdate(update.VersionId);
            }
            else
            {
                try
                {
                    DownloadInProgress.Value = true;

                    Status.Value = "Downloading update...";
                    var updatePath = await _updateSvc.DownloadUpdate(update.Source, update.VersionId, UpdateAssetTypes.ServerUpdate);

                    Status.Value = "Uploading update to target server...";
                    res = await agent.Model.StartServerUpdateFromFile(update.Version, updatePath);

                    Status.Value = null;
                }
                finally
                {
                    DownloadInProgress.Value = false;
                }
            }

            if (res == null)
            {
                Status.Value = "Can't start server update: Empty response";
                StatusHasError.Value = true;
            }
            else if (!res.Started)
            {
                Status.Value = $"Can't start server update: {res.ErrorMsg}";
                StatusHasError.Value = true;
            }
        }

        private async Task DiscardUpdateResultAsync()
        {
            try
            {
                if (_isRemoteUpdate)
                {
                    if (_remoteAgent.Model.AccessManager.CanControlServerUpdate())
                        await _remoteAgent.Model.DiscardServerUpdateResult();
                }
                else
                {
                    _updateSvc.DiscardUpdateResult();
                }
            }
            catch (Exception ex)
            {
                var errMsg = _isRemoteUpdate
                    ? $"Failed to discard update result for server '{_remoteAgent?.Name}'"
                    : "Failed to discard update result";
                _logger.Error(ex, errMsg);
            }
        }

        private async Task DownloadSetupAsync(AppUpdateViewModel update)
        {
            UpdateListGuiEnabled.Value = false;
            DownloadInProgress.Value = true;
            Status.Value = string.Empty;
            StatusHasError.Value = false;

            try
            {
                Status.Value = "Downloading setup...";
                var setupPath = await _updateSvc.DownloadUpdate(update.Source, update.VersionId, UpdateAssetTypes.Setup);

                Status.Value = "Saving setup...";
                var dlg = new SaveFileDialog
                {
                    FileName = Path.GetFileName(setupPath),
                    InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
                    Filter = "Executable files (*.exe)|*.exe"
                };
                var res = dlg.ShowDialog();
                if (res == true)
                {
                    var filePath = dlg.FileName;
                    var folderPath = Path.GetDirectoryName(dlg.FileName);
                    File.Copy(setupPath, filePath, true);
                    WinExplorerHelper.ShowFolder(folderPath);
                }

                Status.Value = null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to download setup");
                Status.Value = "Download failed unexpectedly. See logs...";
                StatusHasError.Value = true;
            }

            DownloadInProgress.Value = false;
            UpdateListGuiEnabled.Value = true;
        }


        internal class AppUpdateViewModel
        {
            public string Source { get; }

            public string VersionId { get; }

            public string Version { get; }

            public string ReleaseDate { get; }

            public string MinVersion { get; }

            public string Changelog { get; }

            public bool IsStable { get; }

            public string VersionDateStr { get; }

            public string AppType { get; private set; }

            public bool ShowSource { get; }

            public bool HasUpdate { get; private set; }

            public bool HasSetup { get; private set; }

            public bool IsRemoteUpdate { get; }

            public bool SupportedByMinVersion { get; }


            public AppUpdateViewModel(AppUpdateEntry entry, string currentVersion)
            {
                Source = entry.SrcId;
                ShowSource = InitShowSource(entry.SrcId);
                InitAssets(entry.AvailableAssets, UpdateAssetTypes.TerminalUpdate);

                VersionId = entry.VersionId;
                Version = entry.Info.ReleaseVersion;
                ReleaseDate = entry.Info.ReleaseDate;
                MinVersion = entry.Info.MinVersion;
                Changelog = entry.Info.Changelog;
                IsStable = entry.IsStable;
                VersionDateStr = InitVersionDateStr(Version, ReleaseDate);
                SupportedByMinVersion = AppVersionInfo.CompareVersions(currentVersion, MinVersion) >= 0;
            }

            public AppUpdateViewModel(AppUpdateEntry entry, ServerUpdateInfo serverUpdate, string currentVersion)
            {
                if (entry == null)
                {
                    Source = "From AlgoServer";
                    ShowSource = true;
                    InitAssets(new[] { UpdateAssetTypes.ServerUpdate }, UpdateAssetTypes.ServerUpdate);
                }
                else
                {
                    Source = entry.SrcId;
                    ShowSource = InitShowSource(entry.SrcId);
                    InitAssets(entry.AvailableAssets, UpdateAssetTypes.ServerUpdate);
                }

                if (serverUpdate != null)
                {
                    IsRemoteUpdate = true;
                    VersionId = serverUpdate.ReleaseId;
                    Version = serverUpdate.Version;
                    ReleaseDate = serverUpdate.ReleaseDate;
                    MinVersion = serverUpdate.MinVersion;
                    Changelog = serverUpdate.Changelog;
                    IsStable = serverUpdate.IsStable;
                }
                else
                {
                    VersionId = entry.VersionId;
                    Version = entry.Info.ReleaseVersion;
                    ReleaseDate = entry.Info.ReleaseDate;
                    MinVersion = entry.Info.MinVersion;
                    Changelog = entry.Info.Changelog;
                    IsStable = entry.IsStable;
                }
                VersionDateStr = InitVersionDateStr(Version, ReleaseDate);
                SupportedByMinVersion = AppVersionInfo.CompareVersions(currentVersion, MinVersion) >= 0;
            }


            private static bool InitShowSource(string srcId)
                // don't display main source on gui
                => srcId != AutoUpdateService.MainSourceName;

            private static string InitVersionDateStr(string version, string releaseDate) => $"{version} ({releaseDate})";


            private void InitAssets(IEnumerable<UpdateAssetTypes> availableAssets, UpdateAssetTypes updateType)
            {
                HasUpdate = availableAssets.Contains(updateType);
                HasSetup = availableAssets.Contains(UpdateAssetTypes.Setup);
                AppType = string.Join(", ", availableAssets);
            }
        }

        private class LoadUpdatesErrorEquailityComparer : IEqualityComparer<string>
        {
            public static LoadUpdatesErrorEquailityComparer Instance { get; } = new LoadUpdatesErrorEquailityComparer();


            public bool Equals(string a, string b)
            {
                var indexA = a.IndexOf("RetryAfter=");
                var indexB = b.IndexOf("RetryAfter=");
                if (indexA != -1 && indexB != -1)
                {
                    var subA = a[..indexA];
                    var subB = b[..indexB];
                    return subA == subB;
                }

                return indexA == indexB;
            }

            public int GetHashCode(string obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
