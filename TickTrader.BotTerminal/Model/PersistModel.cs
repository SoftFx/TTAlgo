using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class PersistModel
    {
        private List<IPersistController> _controllers = new List<IPersistController>();


        public AuthStorageModel AuthSettingsStorage { get; }

        public SettingsStorage<PreferencesStorageModel> PreferencesStorage { get; }

        public ProfileManager ProfileManager { get; }


        public PersistModel()
        {
            var loginController = CreateStorageController<AuthStorageModel>("UserAuthSettings", EnvService.Instance.ProtectedUserDataStorage);
            AuthSettingsStorage = loginController.Value;
            var settingsController = CreateStorageController<PreferencesStorageModel>("Preferences", EnvService.Instance.UserDataStorage);
            PreferencesStorage = new SettingsStorage<PreferencesStorageModel>(settingsController.Value);
            ProfileManager = new ProfileManager(CreateStorageController<ProfileStorageModel>("stub.profile", EnvService.Instance.ProfilesCacheStorage));
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
    }
}
