using NLog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class ProfileManager
    {
        private const int LoopDelay = 50;


        public const string DefaultProfileFileName = "default.profile";


        private static readonly TimeSpan SaveDelay = TimeSpan.FromSeconds(2);


        public static string DefaultProfilePath = Path.Combine(EnvService.Instance.ProfilesCacheFolder, DefaultProfileFileName);


        private Logger _logger;
        private ObjectPersistController<ProfileStorageModel> _storageController;
        private bool _isSaving;


        public string Server { get; private set; }

        public string Login { get; private set; }

        public string CurrentProfileFileName { get; private set; }

        public string CurrentProfilePath => Path.Combine(EnvService.Instance.ProfilesCacheFolder, CurrentProfileFileName);

        public ProfileStorageModel CurrentProfile => _storageController.Value;


        public event Action ProfileUpdated;
        public Action<ProfileStorageModel> SaveProfileSnapshot = delegate { };


        public ProfileManager(ObjectPersistController<ProfileStorageModel> storageController)
        {
            _storageController = storageController;

            _logger = NLog.LogManager.GetCurrentClassLogger();

            _isSaving = false;
            CurrentProfileFileName = DefaultProfileFileName;

            if (!File.Exists(CurrentProfilePath))
            {
                CurrentProfile.Save();
            }
        }


        public async Task<bool> StopCurrentProfile(string server, string login)
        {
            if (Server == server && Login == login)
            {
                return false;
            }

            await StopCurrentProfile();
            return true;
        }

        public async Task StopCurrentProfile()
        {
            await _storageController.Close();
            while (_isSaving)
            {
                await Task.Delay(LoopDelay);
            }
        }

        public async void Stop()
        {
            await StopCurrentProfile();
        }

        public void LoadCachedProfile(string server, string login)
        {
            if (string.IsNullOrEmpty(server))
                throw new ArgumentException("Invalid server name");

            if (string.IsNullOrEmpty(login))
                throw new ArgumentException("Invalid login");

            Server = server;
            Login = login;
            CurrentProfileFileName = $"{server}_{login}.profile";

            OpenCachedProfile();
        }

        public void LoadUserProfile(string profileName)
        {
            OpenUserProfile(Path.Combine(EnvService.Instance.UserProfilesFolder, $"{profileName}.profile"));
        }

        public void SaveUserProfile(string profileName)
        {
            try
            {
                SaveCurrentProfile(Path.Combine(EnvService.Instance.UserProfilesFolder, $"{profileName}.profile"));
            }
            catch (Exception ex)
            {
                _logger.Error($"Can't save user profile '{profileName}': {ex.Message}");
            }
        }


        private void OnProfileUpdated()
        {
            ProfileUpdated?.Invoke();
        }

        private void OpenCachedProfile()
        {
            if (!File.Exists(CurrentProfilePath) && File.Exists(DefaultProfilePath))
            {
                File.Copy(DefaultProfilePath, CurrentProfilePath);
            }

            OpenCurrentProfile();
        }

        private void OpenUserProfile(string newProfilePath)
        {
            if (File.Exists(newProfilePath))
            {
                File.Copy(newProfilePath, CurrentProfilePath, true);
            }

            OpenCurrentProfile();
        }

        private void OpenCurrentProfile()
        {
            try
            {
                _storageController.SetFilename(CurrentProfilePath);
                _storageController.Reopen();

                OnProfileUpdated();

                SaveLoop();
            }
            catch (Exception ex)
            {
                _logger.Error($"Can't open current profile {CurrentProfilePath}: {ex.Message}");
            }
        }

        private void SaveCurrentProfile(string path)
        {
            File.Copy(CurrentProfilePath, path, true);
        }

        private async void SaveLoop()
        {
            _isSaving = true;

            var lastSaveTime = DateTime.Now;

            while (_storageController.CanSave)
            {
                if (DateTime.Now - lastSaveTime > SaveDelay)
                {
                    try
                    {
                        SaveProfileSnapshot(CurrentProfile);
                        CurrentProfile.Save();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Failed to save current profile");
                    }
                    lastSaveTime = DateTime.Now;
                }

                await Task.Delay(LoopDelay);
            }

            _isSaving = false;
        }
    }
}
