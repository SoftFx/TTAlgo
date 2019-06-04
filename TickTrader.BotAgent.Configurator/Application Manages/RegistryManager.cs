using Microsoft.Win32;
using System;
using System.IO;


namespace TickTrader.BotAgent.Configurator
{
    public class RegistryManager
    {
        private const string ApplicationPathKey = "";

        //public List<RegistryKey> _applicationPaths;

        private RegistryKey _baseKey = Registry.LocalMachine.OpenSubKey("SOFTWARE");
        private string _configPath = Path.Combine("WebAdmin", "appsettings.json");
        private string _botAgentPath;
        private string _botAgentConfigPath = string.Empty;

        public string RegistryApplicationName => "TickTrader Bot Agent";
        public string ApplicationName => "TickTrader.BotAgent";

        public RegistryManager()
        {
            //_applicationPaths = new List<RegistryKey>();
        }

        public StreamReader GetConfigurationStreamReader()
        {
            if (string.IsNullOrEmpty(_botAgentConfigPath))
                InitBotAgentPaths();

            return new StreamReader(_botAgentConfigPath);
        }

        public StreamWriter GetConfigurationStreamWriter()
        {
            if (string.IsNullOrEmpty(_botAgentConfigPath))
                InitBotAgentPaths();

            return new StreamWriter(_botAgentConfigPath);
        }

        private void InitBotAgentPaths()
        {
            //SearchApplicationPath(_baseKey, ApplicationName);

            //if (_applicationPaths.Count > 1)
            //    throw new Exception($"More than one {ApplicationName} version found");

            //if (_applicationPaths.Count == 0)
            //    throw new Exception($"{ApplicationName} not found");

            var applicationKey = _baseKey.OpenSubKey(RegistryApplicationName);
            if (applicationKey == null)
                throw new Exception($"{RegistryApplicationName} not found");

            _botAgentPath = GetApplicationPath(applicationKey);
            _botAgentConfigPath = GetBotAgentConfigPath();
        }

        private string GetBotAgentConfigPath()
        {
            var path = Path.Combine(_botAgentPath, _configPath);

            if (!File.Exists(path))
                throw new Exception($"{_configPath} not fount in {_botAgentPath} folder");

            return path;
        }

        private string GetApplicationPath(RegistryKey key)
        {
            var path = key.GetValue(ApplicationPathKey);

            if (path == null)
                throw new Exception($"{RegistryApplicationName} path was not found");

            return path as string;
        }

        //private void SearchApplicationPath(RegistryKey currentKey, string applicationKey)
        //{
        //    if (currentKey.Name.Contains(applicationKey) && !currentKey.Name.Contains("Uninstall"))
        //        _applicationPaths.Add(currentKey);

        //    foreach (var subKeyName in currentKey.GetSubKeyNames())
        //    {
        //        try
        //        {
        //            SearchApplicationPath(currentKey.OpenSubKey(subKeyName), applicationKey);
        //        }
        //        catch (SecurityException) { }
        //        catch (Exception ex)
        //        {
        //            MessageBoxManager.ErrorBox(ex.Message);
        //        }
        //    }
        //}
    }
}
