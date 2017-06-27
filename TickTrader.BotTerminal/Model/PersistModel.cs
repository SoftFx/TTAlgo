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
        private ObjectPersistController<ProfileStorageModel> _profileStorageController;


        public AuthStorageModel AuthSettingsStorage { get; private set; }

        public ProfileStorageModel ProfileStorage { get; private set; }


        public PersistModel()
        {
            var loginController = CreateStorageController<AuthStorageModel>("UserAuthSettings", EnvService.Instance.ProtectedUserDataStorage);
            AuthSettingsStorage = loginController.Value;
            _profileStorageController = CreateStorageController<ProfileStorageModel>("CurrentProfile", EnvService.Instance.UserDataStorage);
            ProfileStorage = _profileStorageController.Value;
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
