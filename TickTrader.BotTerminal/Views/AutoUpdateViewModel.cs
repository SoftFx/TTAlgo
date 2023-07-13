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
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<AutoUpdateViewModel>();
        private static UpdateAssetTypes[] _terminalAssets = new[] { UpdateAssetTypes.TerminalUpdate, UpdateAssetTypes.Setup };
        private static UpdateAssetTypes[] _serverAssets = new[] { UpdateAssetTypes.ServerUpdate, UpdateAssetTypes.Setup };

        private readonly VarContext _context = new();
        private readonly AutoUpdateService _updateSvc;
        private readonly System.Action _exitCallback;
        private readonly AlgoAgentViewModel _remoteAgent;


        public StrProperty CurrentVersion { get; }

        public BoolProperty GuiEnabled { get; }

        public BoolProperty UpdatesLoaded { get; }

        public ObservableCollection<AppUpdateViewModel> AvailableUpdates { get; } = new();

        public Property<AppUpdateViewModel> SelectedUpdate { get; }

        public BoolProperty HasSelectedUpdate { get; }

        public StrProperty Status { get; }

        public BoolProperty StatusHasError { get; }

        public BoolProperty UpdateInProgress { get; }

        public bool IsRemoteUpdate { get; }


        private AutoUpdateViewModel()
        {
            CurrentVersion = _context.AddStrProperty();
            GuiEnabled = _context.AddBoolProperty();
            UpdatesLoaded = _context.AddBoolProperty();
            HasSelectedUpdate = _context.AddBoolProperty();
            SelectedUpdate = _context.AddProperty<AppUpdateViewModel>().AddPostTrigger(_ => OnSelectedUpdateChanged());
            Status = _context.AddStrProperty();
            StatusHasError = _context.AddBoolProperty();
            UpdateInProgress = _context.AddBoolProperty();
        }

        public AutoUpdateViewModel(AutoUpdateService updateSvc, System.Action exitCallback) : this()
        {
            _updateSvc = updateSvc;
            _exitCallback = exitCallback;

            DisplayName = $"Auto Update - {EnvService.Instance.ApplicationName}";
            IsRemoteUpdate = false;
            var appVersion = AppVersionInfo.Current;
            CurrentVersion.Value = $"{appVersion.Version} ({appVersion.BuildDate})";

            _ = LoadUpdatesAsync();
        }

        public AutoUpdateViewModel(AutoUpdateService updateSvc, BotAgentViewModel remoteAgent) : this()
        {
            _updateSvc = updateSvc;
            _remoteAgent = remoteAgent.Agent;

            DisplayName = $"Auto Update - {remoteAgent.Agent.Name}";
            IsRemoteUpdate = true;
            _remoteAgent.Model.AccessLevelChanged += OnAgentAccessLevelChanged;

            _ = LoadUpdatesAsync();
        }


        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            if (close)
            {
                if (_remoteAgent != null)
                    _remoteAgent.Model.AccessLevelChanged -= OnAgentAccessLevelChanged;
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

        public void InstallManual()
        {
            _ = DownloadAndRunSetupAsync(SelectedUpdate.Value);
        }


        private void OnSelectedUpdateChanged()
        {
            if (SelectedUpdate == null)
                return;

            HasSelectedUpdate.Value = SelectedUpdate.Value != null;
        }

        private void OnAgentAccessLevelChanged()
        {
            _ = LoadUpdatesAsync();
        }

        private async Task LoadUpdatesAsync(bool forced = false)
        {
            GuiEnabled.Value = false;
            UpdatesLoaded.Value = false;

            try
            {
                if (IsRemoteUpdate)
                    await LoadRemoteUpdatesInternal(forced);
                else
                    await LoadLocalUpdatesInternal(forced);
            }
            catch (Exception ex)
            {
                var errMsg = IsRemoteUpdate
                    ? $"Failed to load updates for server {_remoteAgent?.Name}"
                    : "Failed to load updates";
                _logger.Error(ex, errMsg);
            }

            UpdatesLoaded.Value = true;
            GuiEnabled.Value = true;
        }

        private async Task LoadLocalUpdatesInternal(bool forced)
        {
            var updates = await _updateSvc.GetUpdates(forced);

            AvailableUpdates.Clear();
            foreach (var update in updates)
                if (update.AvailableAssets.Any(a => _terminalAssets.Contains(a)))
                    AvailableUpdates.Add(new AppUpdateViewModel(update));
        }

        private async Task LoadRemoteUpdatesInternal(bool forced)
        {
            var agent = _remoteAgent;
            ServerUpdateList serverUpdates = null;
            if (!agent.Model.VersionSpec.SupportsAutoUpdate)
            {
                CurrentVersion.Value = "AutoUpdate not supported";
            }
            else
            {
                var versionInfo = await agent.Model.GetServerVersion();
                CurrentVersion.Value = $"{versionInfo.Version} ({versionInfo.ReleaseDate})";
                serverUpdates = await agent.Model.GetServerUpdateList(forced);
            }

            var updates = await _updateSvc.GetUpdates(forced);
            AvailableUpdates.Clear();
            foreach (var update in updates)
                if (update.AvailableAssets.Any(a => _serverAssets.Contains(a)))
                {
                    ServerUpdateInfo serverUpd = null;
                    // Match releases from main repo to use server-side download
                    if (update.SrcId == AutoUpdateService.MainSourceName && serverUpdates != null)
                        serverUpd = serverUpdates.Updates.FirstOrDefault(u => u.ReleaseId == update.VersionId);

                    AvailableUpdates.Add(new AppUpdateViewModel(update, serverUpd));
                }
        }

        private async Task InstallUpdateAsync()
        {
            GuiEnabled.Value = false;
            UpdateInProgress.Value = true;
            Status.Value = string.Empty;
            StatusHasError.Value = false;

            try
            {
                var update = SelectedUpdate.Value;
                if (IsRemoteUpdate)
                    await InstallRemoteUpdateInternal(update);
                else
                    await InstallLocalUpdateInternal(update);
            }
            catch (Exception ex)
            {
                var errMsg = IsRemoteUpdate
                    ? $"Failed to install update for server {_remoteAgent?.Name}"
                    : "Failed to install update";
                _logger.Error(ex, errMsg);
                Status.Value = "Update failed unexpectedly. See logs...";
                StatusHasError.Value = true;
            }

            UpdateInProgress.Value = false;
            GuiEnabled.Value = true;
        }

        private async Task InstallLocalUpdateInternal(AppUpdateViewModel update)
        {
            Status.Value = "Downloading update...";
            var updatePath = await _updateSvc.DownloadUpdate(update.Entry, UpdateAssetTypes.TerminalUpdate);

            Status.Value = "Extracting update...";
            var updateDir = Path.Combine(EnvService.Instance.UpdatesFolder, update.Version);
            UpdateHelper.ExtractUpdate(updatePath, updateDir);

            Status.Value = "Starting update...";
            var updateParams = new UpdateParams
            {
                AppTypeCode = (int)UpdateAppTypes.Terminal,
                InstallPath = AppInfoProvider.Data.AppInfoFolder,
                UpdatePath = updateDir,
                FromVersion = AppVersionInfo.Current.Version,
            };
            var startSuccess = await UpdateHelper.StartUpdate(EnvService.Instance.UpdatesFolder, updateParams, true);
            if (!startSuccess)
            {
                var state = UpdateHelper.LoadUpdateState(EnvService.Instance.UpdatesFolder);
                var error = state.HasErrors ? UpdateHelper.FormatStateError(state) : "Unexpected update error";
                Status.Value = error;
                StatusHasError.Value = true;
            }
            else
            {
                _exitCallback?.Invoke();
                Status.Value = null;
            }
        }

        private async Task InstallRemoteUpdateInternal(AppUpdateViewModel update)
        {
            var agent = _remoteAgent;
            if (update.ServerUpdate != null)
            {
                await agent.Model.StartServerUpdate(update.ServerUpdate.ReleaseId);
            }
            else
            {
                Status.Value = "Downloading update...";
                var updatePath = await _updateSvc.DownloadUpdate(update.Entry, UpdateAssetTypes.ServerUpdate);

                Status.Value = "Uploading update to target server...";
                await agent.Model.StartServerUpdateFromFile(update.Version, updatePath);

                Status.Value = null;
            }
        }

        private async Task DownloadAndRunSetupAsync(AppUpdateViewModel update)
        {
            GuiEnabled.Value = false;
            UpdateInProgress.Value = true;
            Status.Value = string.Empty;
            StatusHasError.Value = false;

            try
            {
                Status.Value = "Downloading setup...";
                var setupPath = await _updateSvc.DownloadUpdate(update.Entry, UpdateAssetTypes.Setup);

                if (IsRemoteUpdate)
                {
                    Status.Value = "Saving setup...";
                    var dlg = new SaveFileDialog
                    {
                        FileName = Path.GetFileName(setupPath),
                        InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
                        Filter = "Executable files (*.exe)|*.exe"
                    };
                    var res = dlg.ShowDialog();
                    var filePath = dlg.FileName;
                    var folderPath = Path.GetDirectoryName(dlg.FileName);
                    File.Copy(setupPath, filePath, true);
                    WinExplorerHelper.ShowFolder(folderPath);
                }
                else
                {
                    Status.Value = "Starting setup...";
                    Process.Start(new ProcessStartInfo(setupPath) { UseShellExecute = true });
                }

                Status.Value = null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to download/run setup");
                Status.Value = "Download/run failed unexpectedly. See logs...";
                StatusHasError.Value = true;
            }

            UpdateInProgress.Value = false;
            GuiEnabled.Value = true;
        }


        internal class AppUpdateViewModel
        {
            public AppUpdateEntry Entry { get; }

            public ServerUpdateInfo ServerUpdate { get; }

            public string Source { get; }

            public string Version { get; }

            public string ReleaseDate { get; }

            public string MinVersion { get; }

            public string Changelog { get; }

            public string AppType { get; }


            public AppUpdateViewModel(AppUpdateEntry entry)
            {
                Entry = entry;

                Source = entry.SrcId;
                AppType = string.Join(", ", Entry.AvailableAssets);
                Version = entry.Info.ReleaseVersion;
                ReleaseDate = entry.Info.ReleaseDate;
                MinVersion = entry.Info.MinVersion;
                Changelog = entry.Info.Changelog;
            }

            public AppUpdateViewModel(AppUpdateEntry entry, ServerUpdateInfo serverUpdate)
            {
                Entry = entry;
                ServerUpdate = serverUpdate;

                Source = entry.SrcId;
                AppType = string.Join(", ", Entry.AvailableAssets);
                if (serverUpdate != null)
                {
                    Version = serverUpdate.Version;
                    ReleaseDate = serverUpdate.ReleaseDate;
                    MinVersion = serverUpdate.MinVersion;
                    Changelog = serverUpdate.Changelog;
                }
                else
                {
                    Version = entry.Info.ReleaseVersion;
                    ReleaseDate = entry.Info.ReleaseDate;
                    MinVersion = entry.Info.MinVersion;
                    Changelog = entry.Info.Changelog;
                }
            }
        }
    }
}
