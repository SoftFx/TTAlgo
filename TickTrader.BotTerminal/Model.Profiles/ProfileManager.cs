using NLog;
using System;
using System.IO;

namespace TickTrader.BotTerminal
{
    internal class ProfileManager
    {
        public const string CurrentProfileFileName = "current.profile";


        public static string CurrentProfilePath = Path.Combine(EnvService.Instance.ProfilesCacheFolder, CurrentProfileFileName);


        private Logger _logger;
        private ObjectPersistController<ProfileStorageModel> _storageController;


        public string Server { get; private set; }

        public string Login { get; private set; }

        public ProfileStorageModel CurrentProfile => _storageController.Value;


        public event Action ProfileUpdated;


        public ProfileManager(ObjectPersistController<ProfileStorageModel> storageController)
        {
            _storageController = storageController;

            _logger = NLog.LogManager.GetCurrentClassLogger();

            if (!File.Exists(CurrentProfilePath))
            {
                CurrentProfile.Save();
            }
        }


        public void LoadCachedProfile(string server, string login)
        {
            if (Server == server && Login == login)
            {
                return;
            }

            if (!string.IsNullOrEmpty(Server) && !string.IsNullOrEmpty(Login))
            {
                try
                {
                    SaveCurrentProfile(Path.Combine(EnvService.Instance.ProfilesCacheFolder, $"{Server}_{Login}.profile"));
                }
                catch (Exception ex)
                {
                    _logger.Error($"Can't save profile to cache for {Server} {Login}: {ex.Message}");
                }
            }

            Server = server;
            Login = login;

            OpenProfile(Path.Combine(EnvService.Instance.ProfilesCacheFolder, $"{server}_{login}.profile"));
        }

        public void LoadUserProfile(string profileName)
        {
            OpenProfile(Path.Combine(EnvService.Instance.UserProfilesFolder, $"{profileName}.profile"));
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

        private void OpenProfile(string newProfilePath)
        {
            try
            {
                _storageController.Close().Wait();

                File.Delete(CurrentProfilePath);
                if (File.Exists(newProfilePath))
                {
                    File.Copy(newProfilePath, CurrentProfilePath);
                }

                _storageController.Reopen();
                CurrentProfile.Save();

                OnProfileUpdated();
            }
            catch (Exception ex)
            {
                _logger.Error($"Can't open profile {newProfilePath}: {ex.Message}");
            }
        }

        private void SaveCurrentProfile(string path)
        {
            File.Copy(CurrentProfilePath, path, true);
        }
    }
}
