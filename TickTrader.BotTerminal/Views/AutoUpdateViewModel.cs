using Caliburn.Micro;
using Machinarium.Var;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.AppCommon;
using TickTrader.Algo.AppCommon.Update;
using TickTrader.Algo.Core.Lib;
using TickTrader.BotTerminal.Model.AutoUpdate;

namespace TickTrader.BotTerminal
{
    internal sealed class AutoUpdateViewModel : Screen, IWindowModel
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<AutoUpdateViewModel>();

        private readonly VarContext _context = new();
        private readonly AutoUpdateService _updateSvc;
        private readonly System.Action _exitCallback;


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

            UpdatesLoaded = _context.AddBoolProperty();
            HasSelectedUpdate = _context.AddBoolProperty();
            SelectedUpdate = _context.AddProperty<AppUpdateViewModel>().AddPostTrigger(item => HasSelectedUpdate.Value = item != null);
            Status = _context.AddStrProperty();
            StatusHasError = _context.AddBoolProperty();
            UpdateInProgress = _context.AddBoolProperty();

            _ = LoadUpdatesAsync();
        }


        public void RefreshUpdates()
        {
            _ = LoadUpdatesAsync();
        }

        public void InstallSelectedUpdate()
        {
            _ = InstallUpdateAsync(SelectedUpdate.Value);
        }


        private async Task LoadUpdatesAsync()
        {
            UpdatesLoaded.Value = false;

            try
            {
                var updates = await _updateSvc.GetUpdates();

                AvailableUpdates.Clear();
                foreach (var update in updates)
                    AvailableUpdates.Add(new AppUpdateViewModel { Entry = update });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load updates");
            }

            UpdatesLoaded.Value = true;
        }

        private async Task InstallUpdateAsync(AppUpdateViewModel update)
        {
            UpdateInProgress.Value = true;
            Status.Value = string.Empty;
            StatusHasError.Value = false;

            try
            {
                Status.Value = "Downloading update...";
                var updatePath = await _updateSvc.DownloadUpdate(update.Entry);

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
                    ToVersion = update.Version,
                };
                var startSuccess = await UpdateHelper.StartUpdate(EnvService.Instance.UpdatesFolder, updateParams, true);
                if (!startSuccess)
                {
                    var state = UpdateHelper.LoadUpdateState(EnvService.Instance.UpdatesFolder);
                    var error = state.HasErrors ? FormatStateError(state) : "Unexpected update error";
                    Status.Value = error;
                    StatusHasError.Value = true;
                }
                else
                {
                    _exitCallback?.Invoke();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to install update");
                Status.Value = "Update failed unexpectedly. See logs...";
                StatusHasError.Value = true;
            }

            UpdateInProgress.Value = false;
        }

        private static string FormatStateError(UpdateState state)
        {
            var sb = new StringBuilder();

            sb.Append(state.Status.ToString());
            if (state.InitError != UpdateErrorCodes.NoError)
                sb.Append(" - ").Append(state.InitError.ToString());
            sb.AppendLine();
            foreach (var err in state.UpdateErrors)
                sb.AppendLine(err);

            return sb.ToString();
        }


        internal class AppUpdateViewModel
        {
            public AppUpdateEntry Entry { get; init; }

            public string Source => Entry.SrcId;

            public string Version => Entry.Info.ReleaseVersion;

            public string ReleaseDate => Entry.Info.ReleaseDate;

            public string MinVersion => Entry.Info.MinVersion;

            public string Changelog => Entry.Info.Changelog;

            public string AppType => Entry.AppType.ToString();
        }
    }
}
