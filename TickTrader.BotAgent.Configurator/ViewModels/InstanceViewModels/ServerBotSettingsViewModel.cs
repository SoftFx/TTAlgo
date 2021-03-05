using Microsoft.Win32;
using System.IO;

namespace TickTrader.BotAgent.Configurator
{
    public class ServerBotSettingsViewModel : BaseContentViewModel
    {
        private const string FileDialogFilter = "ZIP Folder (.zip)|*.zip";
        private const string DefaultFileDialogExt = ".zip";

        private readonly ServerBotSettingsManager _manager;

        private DelegateCommand _saveCurrentBotSettingsCommand;
        private DelegateCommand _loadCurrentBotSettingsCommand;

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
                {
                    if (_manager.CreateAlgoServerBotSettingZip(dialog.FileName))
                        MessageBoxManager.OkInfo($"{dialog.SafeFileName} save was successfully");
                    else
                        MessageBoxManager.OkError($"{dialog.SafeFileName} save has failed");
                }
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
                {
                    if (_manager.LoadAlgoServerBotSettingZip(dialog.FileName))
                        MessageBoxManager.OkInfo($"{dialog.SafeFileName} load was successfully");
                    else
                        MessageBoxManager.OkError($"{dialog.SafeFileName} load has failed");
                }
            }));

        public ServerBotSettingsViewModel(ServerBotSettingsManager manager) : base(nameof(ServerBotSettingsViewModel))
        {
            _manager = manager;
        }
    }
}
