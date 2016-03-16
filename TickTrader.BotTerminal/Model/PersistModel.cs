using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal.Model
{
    internal class PersistModel
    {
        private List<IPersistController> controllers = new List<IPersistController>();

        public PersistModel()
        {
            var loginStController = new ObjectPersistController<LoginStorageModel>(EnvService.Instance.ProtectedUserDataStorage);
            LoginStorage = loginStController.Value;
        }

        public LoginStorageModel LoginStorage { get; private set; }

        public Task Stop()
        {
            return Task.WhenAll(controllers.Select(c => c.Close()));
        }
    }
}
