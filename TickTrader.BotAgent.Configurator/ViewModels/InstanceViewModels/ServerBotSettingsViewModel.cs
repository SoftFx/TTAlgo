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
        private readonly SpinnerViewModel _spinner;

        private DelegateCommand _saveCurrentBotSettingsCommand;
        private DelegateCommand _loadCurrentBotSettingsCommand;

        public ServerBotSettingsViewModel(ServerBotSettingsManager manager, SpinnerViewModel spinner) : base(nameof(ServerBotSettingsViewModel))
        {
            _manager = manager;
            _spinner = spinner;
        }

        public DelegateCommand SaveCurrentBotSettingsCommand => _saveCurrentBotSettingsCommand ?? (
            _saveCurrentBotSettingsCommand = new DelegateCommand(obj =>
            {
                var dialog = new SaveFileDialog
                {
                    InitialDirectory = _manager.CurrentServer.FolderPath,
                    FileName = $"{_manager.CurrentServer.ServiceId } ({_manager.CurrentServer.Version }) Bot settings",
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
                    InitialDirectory = _manager.CurrentServer.FolderPath,
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

            var result = isSaveProcess ? _manager.CreateAlgoServerBotSettingZip(path) : _manager.LoadAlgoServerBotSettingZip(path);

            _spinner.Stop();

            if (result)
                Application.Current.Dispatcher.BeginInvoke(new Action<string>(MessageBoxManager.OkInfo), $"{messagePath} {(isSaveProcess ? "saved" : "loaded")} successfully");
            else
                Application.Current.Dispatcher.BeginInvoke(new Action<string>(MessageBoxManager.OkError), $"Failed to {(isSaveProcess ? "save" : "load")} {messagePath}");
        }
    }
}
