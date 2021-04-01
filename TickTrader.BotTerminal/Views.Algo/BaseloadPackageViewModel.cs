using Caliburn.Micro;
using Machinarium.ObservableCollections;
using Machinarium.Qnil;
using Machinarium.Var;
using NLog;
using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using TickTrader.Algo.ServerControl;

namespace TickTrader.BotTerminal
{
    internal enum LoadPackageMode
    {
        Upload,
        Download,
    }

    internal abstract class BaseloadPackageViewModel : Screen, IWindowModel
    {
        protected static ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        protected const string FileNameWatcherTemplate = "*.ttalgo";

        protected readonly ObservableRangeCollection<string> _localPackages;
        protected readonly ProgressViewModel _progressModel;

        protected readonly VarContext _varContext = new VarContext();

        private readonly LoadPackageMode _mode;

        private FileSystemWatcher _watcher;
        private string _selectedFolder;

        public AlgoAgentViewModel SelectedAlgoServer { get; }

        public ICollectionView SourcePackageCollectionView { get; }

        public Property<string> AlgoServerPackageName { get; }

        public Property<string> LocalPackageName { get; }

        public Property<string> Error { get; }

        public BoolProperty IsEnabled { get; }


        public abstract IProperty<string> SourcePackageName { get; }

        public abstract IProperty<string> TargetPackageName { get; }

        protected abstract ICollection SourceCollection { get; }


        protected abstract Task RunLoadPackageProgress();

        protected abstract string GetFirstSourcePackageName();

        protected abstract string GetTargetPackageName(string packageId);

        protected abstract string GetSourcePackageName(string packageId);


        public string SelectedFolder //TODO add postTrigger and preTrigger to the PropertyClass
        {
            get => _selectedFolder;
            set
            {
                if (_selectedFolder == value)
                    return;

                DeinitWatcher();

                _selectedFolder = value;

                InitWatcher();

                NotifyOfPropertyChange(nameof(SelectedFolder));
            }
        }


        public BaseloadPackageViewModel(AlgoAgentViewModel algoServer, LoadPackageMode mode)
        {
            _mode = mode;

            _progressModel = new ProgressViewModel();
            _localPackages = new ObservableRangeCollection<string>();

            SelectedAlgoServer = algoServer;
            DisplayName = $"{mode} Algo package";
            SourcePackageCollectionView = CollectionViewSource.GetDefaultView(SourceCollection);

            Error = _varContext.AddProperty<string>();
            IsEnabled = _varContext.AddBoolProperty(true);
            LocalPackageName = _varContext.AddProperty<string>();
            AlgoServerPackageName = _varContext.AddProperty<string>();

            _varContext.TriggerOnChange(LocalPackageName.Var, (args) =>
            {
                if (_mode == LoadPackageMode.Upload)
                    RefreshTargetName();
            });

            _varContext.TriggerOnChange(AlgoServerPackageName.Var, (args) =>
            {
                if (_mode == LoadPackageMode.Download)
                    RefreshTargetName();
            });

            SelectedAlgoServer.Packages.Updated += UpdateAgentPackage;
        }


        public async void Ok()
        {
            try
            {
                IsEnabled.Value = false;

                if (!LocalPackageName.HasValue || !AlgoServerPackageName.HasValue)
                    throw new Exception("Please select a Algo package.");

                await RunLoadPackageProgress();

                TryClose();
            }
            catch (ConnectionFailedException ex)
            {
                Error.Value = $"Error connecting to agent: {SelectedAlgoServer.Name}";
                _logger.Error(ex, Error.DisplayValue);
            }
            catch (Exception ex)
            {
                Error.Value = $"{ex.Message} Failed to {_mode} Algo package";
                _logger.Error(ex, Error.DisplayValue);
            }

            IsEnabled.Value = true;
        }

        public void Cancel() => TryClose(); // using on UI


        protected void SetStartLocation(string packageName)
        {
            var result = TrySetStartLocation(EnvService.Instance.AlgoRepositoryFolder, packageName) ||
                         TrySetStartLocation(EnvService.Instance.AlgoCommonRepositoryFolder, packageName);

            SourcePackageName.Value = !result || string.IsNullOrEmpty(packageName) ? GetFirstSourcePackageName() : GetSourcePackageName(packageName);
        }

        private void InitWatcher()
        {
            _watcher = new FileSystemWatcher(SelectedFolder, FileNameWatcherTemplate)
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = false,
            };

            _watcher.Created += AddNewPacakgeEventHandling;
            _watcher.Deleted += RemovePackageEventHandling;
            _watcher.Renamed += RenamePackageEventHandling;

            _localPackages.AddRange(Directory.GetFiles(SelectedFolder, FileNameWatcherTemplate).Select(u => Path.GetFileName(u)));
        }

        private void DeinitWatcher()
        {
            if (_watcher == null)
                return;

            _watcher.Created -= AddNewPacakgeEventHandling;
            _watcher.Deleted -= RemovePackageEventHandling;
            _watcher.Renamed -= RenamePackageEventHandling;
            _watcher.Dispose();

            _localPackages.Clear();
        }

        private void AddNewPacakgeEventHandling(object sender, FileSystemEventArgs e) => RunLocalPackageEventHandling(() => _localPackages.Add(e.Name));

        private void RemovePackageEventHandling(object sender, FileSystemEventArgs e) => RunLocalPackageEventHandling(() => _localPackages.Remove(e.Name));

        private void RenamePackageEventHandling(object sender, RenamedEventArgs e) => RunLocalPackageEventHandling(() =>
        {
            var isSelectedName = e.OldName == LocalPackageName.Value;

            _localPackages.Remove(e.OldName);
            _localPackages.Add(e.Name);

            if (isSelectedName)
                LocalPackageName.Value = e.Name;
        });


        private void RunLocalPackageEventHandling(System.Action handling)
        {
            OnUIThread(() =>
            {
                handling();

                if (_mode == LoadPackageMode.Download)
                    RefreshTargetName();
            });
        }

        private void RefreshTargetName()
        {
            TargetPackageName.Value = GenerateTargetFileName(SourcePackageName.Value);
            IsEnabled.Value = SourcePackageName.HasValue;
        }

        private string GenerateTargetFileName(string name) //File name generation as in Windows (a.ttalgo, a - Copy.ttalgo, a - Copy (2).ttalgo)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            var ext = Path.GetExtension(name);
            name = Path.GetFileNameWithoutExtension(name);
            var step = 0;

            do
            {
                var fileName = $"{name}{(step > 0 ? " - Copy" : "")}{(step > 1 ? $" ({step})" : "")}{ext}";

                if (string.IsNullOrEmpty(GetTargetPackageName(fileName)))
                    return fileName;
            }
            while (++step < int.MaxValue);

            return $"{name} - Copy ({DateTime.Now.Ticks}){ext}";
        }

        private bool TrySetStartLocation(string folder, string packageName)
        {
            SelectedFolder = folder;

            return string.IsNullOrEmpty(packageName) ? _localPackages.Count > 0 : !string.IsNullOrEmpty(GetLocalPackageName(packageName));
        }

        protected override void OnDeactivate(bool close)
        {
            DeinitWatcher();
            SelectedAlgoServer.Packages.Updated -= UpdateAgentPackage;

            base.OnDeactivate(close);
        }


        protected string FullPackagePath(string fileName, string folder = null) => Path.Combine(folder ?? SelectedFolder, fileName);

        protected string GetAlgoServerPackageName(string packageId) => SelectedAlgoServer.PackageList.FirstOrDefault(p => p.FileName == packageId)?.FileName;

        protected string GetLocalPackageName(string packageId) => _localPackages.FirstOrDefault(u => u.Equals(packageId, StringComparison.InvariantCultureIgnoreCase));


        private void UpdateAgentPackage(ListUpdateArgs<AlgoPackageViewModel> args) => RefreshTargetName();
    }
}
