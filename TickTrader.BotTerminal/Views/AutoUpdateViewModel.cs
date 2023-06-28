using Caliburn.Micro;
using Machinarium.Var;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TickTrader.BotTerminal.Model.AutoUpdate;

namespace TickTrader.BotTerminal
{
    internal sealed class AutoUpdateViewModel : Screen, IWindowModel
    {
        private readonly VarContext _context = new();
        private readonly AutoUpdateService _updateSvc;


        public BoolProperty UpdatesLoaded { get; }

        public ObservableCollection<AppUpdateViewModel> AvailableUpdates { get; } = new();

        public Property<AppUpdateViewModel> SelectedUpdate { get; }

        public BoolProperty CanInstallSelectedUpdate { get; }


        public AutoUpdateViewModel(AutoUpdateService updateSvc)
        {
            _updateSvc = updateSvc;

            UpdatesLoaded = _context.AddBoolProperty();
            CanInstallSelectedUpdate = _context.AddBoolProperty();
            SelectedUpdate = _context.AddProperty<AppUpdateViewModel>().AddPostTrigger(item => CanInstallSelectedUpdate.Value = item != null);

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

            await Task.Delay(1000); // dummy

            var updates = await _updateSvc.GetUpdates();

            AvailableUpdates.Clear();
            foreach (var update in updates)
                AvailableUpdates.Add(new AppUpdateViewModel { Entry = update });

            UpdatesLoaded.Value = true;
        }

        private async Task InstallUpdateAsync(AppUpdateViewModel update)
        {
            //var dstPath = EnvService.Instance.UpdatesCache;
            //await _updateSvc.DownloadUpdate(update.Entry, dstPath);
        }


        internal class AppUpdateViewModel
        {
            public AppUpdateEntry Entry { get; init; }

            public string Source => Entry.SrcId;

            public string Version => Entry.Info.ReleaseVersion;

            public string ReleaseDate => Entry.Info.ReleaseDate;

            public string MinVersion => Entry.Info.MinVersion;

            public string Changelog => Entry.Info.Changelog;
        }
    }
}
