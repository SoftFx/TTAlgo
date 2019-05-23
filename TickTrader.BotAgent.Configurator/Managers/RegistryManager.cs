using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotAgent.Configurator
{
    public class RegistryManager
    {
        public const string ApplicationName = "TickTrader Bot Agent";

        public List<object> _applicationPaths;

        private RegistryKey _baseKey = Registry.LocalMachine;


        public RegistryManager()
        {
            _applicationPaths = new List<object>();
        }

        public void FindAllPathApplication(string applicationName = ApplicationName)
        {
            SearchApplicationPath(_baseKey, ApplicationName);
        }

        private void SearchApplicationPath(RegistryKey currentKey, string applicationKey)
        {
            if (currentKey.Name == applicationKey)
                _applicationPaths.Add(currentKey);

            foreach (var subKeyName in currentKey.GetSubKeyNames())
            {
                try
                {
                    SearchApplicationPath(currentKey.OpenSubKey(subKeyName), applicationKey);
                }
                catch { }
            }
        }
    }
}
