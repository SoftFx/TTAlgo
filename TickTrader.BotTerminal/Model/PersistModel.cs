using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotTerminal
{
    internal class PersistModel
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<PersistModel>();

        private List<IPersistController> _controllers = new List<IPersistController>();


        public AuthStorageModel AuthSettingsStorage { get; }

        public SettingsStorage<PreferencesStorageModel> PreferencesStorage { get; }

        public ProfileManager ProfileManager { get; }

        public BotAgentStorageModel BotAgentStorage { get; }


        public PersistModel()
        {
            var loginController = CreateStorageController<AuthStorageModel>("UserAuthSettings", EnvService.Instance.ProtectedUserDataStorage);
            AuthSettingsStorage = loginController.Value;
            var preferencesController = CreateStorageController<PreferencesStorageModel>("preferences.json", EnvService.Instance.UserDataStorage);
            PreferencesStorage = new SettingsStorage<PreferencesStorageModel>(preferencesController.Value);
            ProfileManager = new ProfileManager(CreateStorageController<ProfileStorageModel>("stub.profile", EnvService.Instance.ProfilesCacheStorage));
            var botAgentsLoginController = CreateStorageController<BotAgentStorageModel>("BotAgentsAuth", EnvService.Instance.ProtectedUserDataStorage);
            BotAgentStorage = botAgentsLoginController.Value;

            MigrateOldPreferences();
        }


        public Task Stop()
        {
            return Task.WhenAll(_controllers.Select(c => c.Close()));
        }

        public ObjectPersistController<T> CreateStorageController<T>(string fileName, IObjectStorage storage)
            where T : class, IPersistableObject<T>, new()
        {
            var storageController = new ObjectPersistController<T>(fileName, storage);
            _controllers.Add(storageController);
            return storageController;
        }

        public Task DisableStorageController(IPersistController storageController)
        {
            _controllers.Remove(storageController);
            return storageController.Close();
        }


        private void MigrateOldPreferences()
        {
            try
            {
                const string oldFilename = "Preferences";
                var oldPreferencesPath = Path.Combine(EnvService.Instance.AppDataFolder, oldFilename);
                if (File.Exists(oldPreferencesPath))
                {
                    var xmlStorage = new XmlObjectStorage(new FolderBinStorage(EnvService.Instance.AppDataFolder));
                    var oldSettigns = xmlStorage.Load<PreferencesStorageModel>(oldFilename);

                    var settings = PreferencesStorage.StorageModel;
                    settings.EnableSounds = oldSettigns.EnableSounds;
                    settings.RestartBotsOnStartup = oldSettigns.RestartBotsOnStartup;
                    settings.EnableNotifications = oldSettigns.EnableNotifications;
                    settings.Theme =oldSettigns.Theme;
                    settings.Save();

                    File.Delete(oldPreferencesPath);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to migrate old preferences");
            }
        }
    }
}
