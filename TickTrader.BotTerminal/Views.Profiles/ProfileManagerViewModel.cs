using Caliburn.Micro;
using Machinarium.Qnil;
using Microsoft.Win32;
using NLog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class ProfileManagerViewModel : PropertyChangedBase
    {
        private Logger _logger;
        private IProfileLoader _profileLoader;
        private WindowManager _wndManager;
        private CancellationTokenSource _cancelLoadSrc;
        private ProfileRepository _profileRepo;
        private VarDictionary<string, string> _profiles;
        private ProfileManager _profileManager;
        private LocalAlgoAgent2 _agent;
        private bool _canLoadProfile;

        public IObservableList<string> Profiles { get; }

        public bool CanLoadProfile
        {
            get { return _canLoadProfile && _profiles.Count > 0; }
            set
            {
                _canLoadProfile = value;
                NotifyOfPropertyChange(nameof(CanLoadProfile));
            }
        }


        public ProfileManagerViewModel(IShell shell, PersistModel storage)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _profileLoader = shell.ProfileLoader;
            _wndManager = shell.ToolWndManager;
            _profileManager = storage.ProfileManager;
            _agent = shell.Agent;

            _profiles = new VarDictionary<string, string>();
            Profiles = _profiles.OrderBy((k, v) => v).Chain().AsObservable();

            StartRepository();
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
            if (name == null)
                return;

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

            await _agent.IndicatorHost.Start();
        }

        public async Task LoadConnectionProfile(string server, string login, CancellationToken token)
        {
            try
            {
                await _profileManager.StopCurrentProfile();

                token.ThrowIfCancellationRequested();

                _profileManager.LoadCachedProfile(server, login);

                token.ThrowIfCancellationRequested();

                _profileLoader.ReloadProfile(token);

                token.ThrowIfCancellationRequested();

                _profileManager.StartCurrentProfile();
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

                token.ThrowIfCancellationRequested();

                _profileManager.StartCurrentProfile();
            }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to load user profile '{name}'");
            }
        }


        private void StartRepository()
        {
            _profileRepo = new ProfileRepository(EnvService.Instance.UserProfilesFolder);
            _profileRepo.Added += ProfileAdded;
            _profileRepo.Removed += ProfileRemoved;
            _profileRepo.Start();
        }

        private void ProfileAdded(ProfileRepositoryEventArgs args)
        {
            Execute.OnUIThread(() =>
            {
                try
                {
                    _profiles.Add(args.FilePath, args.ProfileName);
                    NotifyOfPropertyChange(nameof(CanLoadProfile));
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            });
        }


        private void ProfileRemoved(ProfileRepositoryEventArgs args)
        {
            Execute.OnUIThread(() =>
            {
                try
                {
                    _profiles.Remove(args.FilePath);
                    NotifyOfPropertyChange(nameof(CanLoadProfile));
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            });
        }
    }
}
