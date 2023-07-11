using Caliburn.Micro;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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
            var appVersion = AppVersionInfo.Current;
            CurrentVersion.Value = $"{appVersion.Version} ({appVersion.BuildDate})";

            _ = LoadUpdatesAsync();
        }

        public AutoUpdateViewModel(AutoUpdateService updateSvc, BotAgentViewModel remoteAgent) : this()
        {
            _updateSvc = updateSvc;
            _remoteAgent = remoteAgent.Agent;

            DisplayName = $"Auto Update - {remoteAgent.Agent.Name}";
            _remoteAgent.Model.AccessLevelChanged += OnAgentAccessLevelChanged;

            _ = LoadRemoteUpdatesAsync();
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
            if (_remoteAgent == null)
            {
                _ = LoadUpdatesAsync(true);
            }
            else
            {
                _ = LoadRemoteUpdatesAsync(true);
            }
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
            _ = InstallUpdateAsync(SelectedUpdate.Value);
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
            _ = LoadRemoteUpdatesAsync();
        }

        private async Task LoadUpdatesAsync(bool forced = false)
        {
            GuiEnabled.Value = false;
            UpdatesLoaded.Value = false;

            try
            {
                var updates = await _updateSvc.GetUpdates(forced);

                AvailableUpdates.Clear();
                foreach (var update in updates)
                    if (update.AvailableAssets.Any(a => _terminalAssets.Contains(a)))
                        AvailableUpdates.Add(new AppUpdateViewModel(update));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load updates");
            }

            UpdatesLoaded.Value = true;
            GuiEnabled.Value = true;
        }

        private async Task LoadRemoteUpdatesAsync(bool forced = false)
        {
            GuiEnabled.Value = false;
            UpdatesLoaded.Value = false;

            var agent = _remoteAgent;
            try
            {
                List<ServerUpdateInfo> serverUpdates = null;
                if (!_remoteAgent.Model.VersionSpec.SupportsAutoUpdate)
                {
                    CurrentVersion.Value = "AutoUpdate not supported";
                }
                else
                {
                    CurrentVersion.Value = await _remoteAgent.Model.GetServerVersion();
                    serverUpdates = await _remoteAgent.Model.GetServerUpdateList(forced);
                }

                var updates = await _updateSvc.GetUpdates(forced);
                AvailableUpdates.Clear();
                foreach (var update in updates)
                    if (update.AvailableAssets.Any(a => _serverAssets.Contains(a)))
                    {
                        ServerUpdateInfo serverUpd = null;
                        // Match releases from main repo to use server-side download
                        if (update.SrcId == AutoUpdateService.MainSourceName && serverUpdates != null)
                            serverUpd = serverUpdates.FirstOrDefault(u => u.ReleaseId == update.VersionId);

                        AvailableUpdates.Add(new AppUpdateViewModel(update, serverUpd));
                    }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to load updates for server {agent?.Name}");
            }

            UpdatesLoaded.Value = true;
            GuiEnabled.Value = true;
        }

        private async Task InstallUpdateAsync(AppUpdateViewModel update)
        {
            GuiEnabled.Value = false;
            UpdateInProgress.Value = true;
            Status.Value = string.Empty;
            StatusHasError.Value = false;

            try
            {
                Status.Value = "Downloading update...";
                var updatePath = await _updateSvc.DownloadUpdate(update.Entry, UpdateAssetTypes.TerminalUpdate);

                Status.Value = "Extracting update...";
                var updateDir = Path.Combine(EnvService.Instance.UpdatesFolder, update.Version);
                if (Directory.Exists(updateDir))
                    Directory.Delete(updateDir, true);
                PathHelper.EnsureDirectoryCreated(updateDir);
                using (var file = File.Open(updatePath, FileMode.Open, FileAccess.Read))
                using (var zip = new ZipArchive(file))
                {
                    zip.ExtractToDirectory(updateDir);
                }

                Status.Value = "Starting update...";
                var updateParams = new UpdateParams
                {
                    AppTypeCode = (int)UpdateAppTypes.Terminal,
                    InstallPath = AppInfoProvider.Data.AppInfoFolder,
                    UpdatePath = updateDir,
                    FromVersion = AppVersionInfo.Current.Version,
                    ToVersion = update.Version,
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
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to install update");
                Status.Value = "Update failed unexpectedly. See logs...";
                StatusHasError.Value = true;
            }

            UpdateInProgress.Value = false;
            GuiEnabled.Value = true;
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

                Status.Value = "Starting setup...";
                Process.Start(new ProcessStartInfo(setupPath) { UseShellExecute = true });

                Status.Value = null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to download/run setup");
                Status.Value = "Install failed unexpectedly. See logs...";
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
