using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotAgent.Configurator
{
    public class ConfigurationModel
    {
        private RegistryManager _registryManager;

        public ConfigurationModel()
        {
            _registryManager = new RegistryManager();

            _registryManager.FindAllPathApplication();
        }
    }
}
