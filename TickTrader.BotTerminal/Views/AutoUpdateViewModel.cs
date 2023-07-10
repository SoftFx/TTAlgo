using Caliburn.Micro;
using Machinarium.Var;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using TickTrader.Algo.AppCommon;
using TickTrader.Algo.AppCommon.Update;
using TickTrader.Algo.AutoUpdate;
using TickTrader.Algo.Core.Lib;
using TickTrader.WpfWindowsSupportLibrary;

namespace TickTrader.BotTerminal
{
    internal sealed class AutoUpdateViewModel : Screen, IWindowModel
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<AutoUpdateViewModel>();

        private readonly VarContext _context = new();
        private readonly AutoUpdateService _updateSvc;
        private readonly System.Action _exitCallback;


        public string CurrentVersion { get; }

        public BoolProperty GuiEnabled { get; }

        public BoolProperty UpdatesLoaded { get; }

        public ObservableCollection<AppUpdateViewModel> AvailableUpdates { get; } = new();

        public Property<AppUpdateViewModel> SelectedUpdate { get; }

        public BoolProperty HasSelectedUpdate { get; }

        public StrProperty Status { get; }

        public BoolProperty StatusHasError { get; }

        public BoolProperty UpdateInProgress { get; }


        public AutoUpdateViewModel(AutoUpdateService updateSvc, System.Action exitCallback)
        {
            _updateSvc = updateSvc;
            _exitCallback = exitCallback;

            DisplayName = "Auto Update";
            var appVersion = AppVersionInfo.Current;
            CurrentVersion = $"{appVersion.Version} ({appVersion.BuildDate})";
            GuiEnabled = _context.AddBoolProperty();
            UpdatesLoaded = _context.AddBoolProperty();
            HasSelectedUpdate = _context.AddBoolProperty();
            SelectedUpdate = _context.AddProperty<AppUpdateViewModel>().AddPostTrigger(_ => OnSelectedUpdateChanged());
            Status = _context.AddStrProperty();
            StatusHasError = _context.AddBoolProperty();
            UpdateInProgress = _context.AddBoolProperty();

            _ = LoadUpdatesAsync();
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

        private async Task LoadUpdatesAsync(bool forced = false)
        {
            GuiEnabled.Value = false;
            UpdatesLoaded.Value = false;

            try
            {
                var updates = await _updateSvc.GetUpdates(forced);

                AvailableUpdates.Clear();
                foreach (var update in updates)
                    AvailableUpdates.Add(new AppUpdateViewModel { Entry = update });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load updates");
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
            public AppUpdateEntry Entry { get; init; }

            public string Source => Entry.SrcId;

            public string Version => Entry.Info.ReleaseVersion;

            public string ReleaseDate => Entry.Info.ReleaseDate;

            public string MinVersion => Entry.Info.MinVersion;

            public string Changelog => Entry.Info.Changelog;

            public string AppType => string.Join(", ", Entry.AvailableAssets);
        }
    }
}
