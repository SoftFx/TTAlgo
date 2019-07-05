using Microsoft.Win32;
using System;
using System.IO;

namespace TickTrader.BotAgent.Configurator
{
    public class RegistryManager : IBotAgentConfigPathHolder
    {
        private const string ApplicationPathKey = "";

        private readonly string _appSettings, _registryAppName, _botAgentConfigPath;

        private RegistryKey _baseKey = Registry.LocalMachine.OpenSubKey("SOFTWARE");
        
        public string BotAgentPath { get; }

        public string BotAgentConfigPath => File.Exists(_botAgentConfigPath) ? _botAgentConfigPath : throw new Exception($"File not found {_botAgentConfigPath}");


        public RegistryManager(string registryApplicationName, string appSettings)
        {
            _registryAppName = registryApplicationName;
            _appSettings = appSettings;

            var applicationKey = _baseKey.OpenSubKey(_registryAppName) ?? throw new Exception($"{_registryAppName} not found");

            BotAgentPath = applicationKey.GetValue(ApplicationPathKey) as string ?? throw new Exception($"{_registryAppName} path not found");

            _botAgentConfigPath = Path.Combine(BotAgentPath, _appSettings);
        }
    }
}
