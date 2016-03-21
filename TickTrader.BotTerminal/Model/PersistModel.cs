using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class PersistModel
    {
        private List<IPersistController> controllers = new List<IPersistController>();

        public PersistModel()
        {
            var loginStController = new ObjectPersistController<AuthStorageModel>("UserAuthSettings", EnvService.Instance.ProtectedUserDataStorage);
            AuthSettingsStorage = loginStController.Value;
        }

        public AuthStorageModel AuthSettingsStorage { get; private set; }

        public Task Stop()
        {
            return Task.WhenAll(controllers.Select(c => c.Close()));
        }
    }
}
