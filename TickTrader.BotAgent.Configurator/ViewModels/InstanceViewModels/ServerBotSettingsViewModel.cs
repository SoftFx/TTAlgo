using Microsoft.Win32;
using System;
using System.Threading;
using System.Windows;
using TickTrader.WpfWindowsSupportLibrary;

namespace TickTrader.BotAgent.Configurator
{
    public class ServerBotSettingsViewModel : BaseContentViewModel
    {
        private const string FileDialogFilter = "ZIP Folder (.zip)|*.zip";
        private const string DefaultFileDialogExt = ".zip";

        private readonly ServerBotSettingsManager _manager;
        private readonly StateServiceViewModel _service;
        private readonly SpinnerViewModel _spinner;

        private DelegateCommand _saveCurrentBotSettingsCommand;
        private DelegateCommand _loadCurrentBotSettingsCommand;


        public bool CanCreateArchive { get; private set; }


        public ServerBotSettingsViewModel(ServerBotSettingsManager manager, SpinnerViewModel spinner, StateServiceViewModel service) : base(nameof(ServerBotSettingsViewModel))
        {
            _manager = manager;
            _spinner = spinner;
            _service = service;

            _service.ChangeServiceStatus += DisablePanelHandler;
            DisablePanelHandler();
        }

        public DelegateCommand SaveCurrentBotSettingsCommand => _saveCurrentBotSettingsCommand ?? (
            _saveCurrentBotSettingsCommand = new DelegateCommand(obj =>
            {
                var dialog = new SaveFileDialog
                {
                    InitialDirectory = ConfiguratorManager.BackupFolder,
                    FileName = _manager.CurrentServer.BackupArchiveName,
                    Filter = FileDialogFilter,
                    DefaultExt = DefaultFileDialogExt
                };

                if (dialog.ShowDialog() == true)
                    ThreadPool.QueueUserWorkItem(RunArchiveBuildProcess, dialog);
            }));

        public DelegateCommand LoadCurrentBotSettingsCommand => _loadCurrentBotSettingsCommand ?? (
            _loadCurrentBotSettingsCommand = new DelegateCommand(obj =>
            {
                var dialog = new OpenFileDialog
                {
                    InitialDirectory = ConfiguratorManager.BackupFolder,
                    Filter = FileDialogFilter,
                    DefaultExt = DefaultFileDialogExt,
                    CheckFileExists = true,
                };

                if (dialog.ShowDialog() == true)
                    ThreadPool.QueueUserWorkItem(RunArchiveBuildProcess, dialog);
            }));

        private void RunArchiveBuildProcess(object o)
        {
            _spinner.Start();

            var isSaveProcess = o is SaveFileDialog;
            var path = isSaveProcess ? ((SaveFileDialog)o).FileName : ((OpenFileDialog)o).FileName;
            var messagePath = isSaveProcess ? path : ((OpenFileDialog)o).SafeFileName;

            var result = isSaveProcess ? _manager.CreateAlgoServerSnapshot(path) : _manager.LoadAlgoServerShapshot(path);

            _spinner.Stop();

            if (result)
                Application.Current.Dispatcher.BeginInvoke(new Action<string>(MessageBoxManager.OkInfo), $"{messagePath} {(isSaveProcess ? "saved" : "loaded")} successfully");
            else
                Application.Current.Dispatcher.BeginInvoke(new Action<string>(MessageBoxManager.OkError), $"Failed to {(isSaveProcess ? "save" : "load")} {messagePath}");
        }

        private void DisablePanelHandler()
        {
            CanCreateArchive = !_service.ServiceRunning;

            OnPropertyChanged(nameof(CanCreateArchive));
        }
    }
}
