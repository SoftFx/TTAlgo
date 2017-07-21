using Microsoft.Win32;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class ProfileManagerViewModel
    {
        private Logger _logger;
        private IProfileLoader _profileLoader;
        private ToolWindowsManager _wndManager;
        private ProfileManager _profileManager;
        private CancellationTokenSource _cancelLoadSrc;


        public ProfileManagerViewModel(IShell shell, PersistModel storage)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _profileLoader = shell.ProfileLoader;
            _wndManager = shell.ToolWndManager;
            _profileManager = storage.ProfileManager;
        }


        public void SetAsDefault()
        {
            _profileManager.SetCurrentProfileAsDefault();
        }

        public void SaveProfile()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = "Profile 1";
            dialog.Filter = "Profile (*.profile) | *.profile";
            dialog.DefaultExt = ".profile";
            dialog.InitialDirectory = EnvService.Instance.UserProfilesFolder;
            if (dialog.ShowDialog() == true)
            {
                _profileManager.SaveUserProfile(dialog.FileName);
            }
        }

        public async void LoadProfile(string name)
        {
            var currentSrc = new CancellationTokenSource();

            try
            {
                if (_cancelLoadSrc != null)
                    _cancelLoadSrc.Cancel();

                _cancelLoadSrc = currentSrc;
            }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to stop previous profile loading!");
            }

            if (currentSrc.IsCancellationRequested)
                return;

            await LoadUserProfile(name, currentSrc.Token);
        }

        public async Task LoadConnectionProfile(string server, string login, CancellationToken token)
        {
            try
            {
                if (!await _profileManager.StopCurrentProfile(server, login))
                {
                    return;
                }

                token.ThrowIfCancellationRequested();

                _profileManager.LoadCachedProfile(server, login);

                token.ThrowIfCancellationRequested();

                _profileLoader.ReloadProfile(token);
            }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to load connection profile for {server} {login}");
            }
        }

        public async Task LoadUserProfile(string name, CancellationToken token)
        {
            try
            {
                await _profileManager.StopCurrentProfile();

                token.ThrowIfCancellationRequested();

                _profileManager.LoadUserProfile(name);

                token.ThrowIfCancellationRequested();

                _profileLoader.ReloadProfile(token);
            }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to load user profile '{name}'");
            }
        }
    }
}
