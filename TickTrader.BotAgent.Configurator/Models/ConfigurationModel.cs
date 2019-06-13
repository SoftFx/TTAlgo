using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TickTrader.BotAgent.Configurator
{
    public class ConfigurationModel
    {
        private ConfigManager _configManager;
        private PortsManager _portsManager;
        private ServiceManager _serviceManager;
        private IBotAgentConfigPathHolder _botAgentHolder;

        private JObject _configurationObject;

        private readonly List<IUploaderModels> _uploaderModels;
        private readonly ConfigurationProperies _settings;

        public CredentialsManager CredentialsManager { get; }

        public SslManager SslManager { get; }

        public ProtocolManager ProtocolManager { get; }

        public ServerManager ServerManager { get; }


        public ConfigurationModel()
        {
            _configManager = new ConfigManager();
            _settings = _configManager.Properties;

            _botAgentHolder = _settings.UseProvider ? (IBotAgentConfigPathHolder)_settings.MultipleAgentProvider : new RegistryManager(_settings[AppProperties.RegistryAppName], _settings[AppProperties.AppSettings]);

            _portsManager = new PortsManager();
            _serviceManager = new ServiceManager(_settings[AppProperties.ServiceName]);

            CredentialsManager = new CredentialsManager("Credentials");
            SslManager = new SslManager("Ssl");
            ProtocolManager = new ProtocolManager("Protocol");
            ServerManager = new ServerManager();

            _uploaderModels = new List<IUploaderModels>() { CredentialsManager, SslManager, ProtocolManager, ServerManager };

            LoadConfiguration();
        }

        public bool StartAgent()
        {
            if (_serviceManager.IsServiceRunning && !MessageBoxManager.YesNoBoxQuestion("The process is already running, restart it?"))
                return false;

            //var hostAndPort = _portsManager.GetHostAndPort(ServerManager.ServerModel.Urls);
            //_portsManager.RegistryPortInFirewall(hostAndPort.Item2, _registryManager.ApplicationName);

            _portsManager.RegistryApplicationInFirewall(_settings[AppProperties.ApplicationName], _botAgentHolder.BotAgentPath);

            if (_serviceManager.IsServiceRunning)
                _serviceManager.ServiceStop();

            PortsManager.CheckPortOpen(ServerManager.ServerModel.Urls);
            _serviceManager.ServiceStart();

            return true;
        }

        public void LoadConfiguration()
        {
            using (var configStreamReader = new StreamReader(_botAgentHolder.BotAgentConfigPath))
            {
                _configurationObject = JObject.Parse(configStreamReader.ReadToEnd());

                foreach (var uploader in _uploaderModels)
                    UploadModel(uploader);
            }
        }

        public void SaveChanges()
        {
            foreach (var uploader in _uploaderModels)
                uploader.SaveConfigurationModels(_configurationObject);

            using (var configStreamWriter = new StreamWriter(_botAgentHolder.BotAgentConfigPath))
            {
                configStreamWriter.Write(_configurationObject.ToString());
            }
        }

        private void UploadModel(IUploaderModels model)
        {
            try
            {
                var args = model.SectionName == "" ? _configurationObject.Values<JProperty>() : _configurationObject[model.SectionName].Children<JProperty>();
                model.UploadModels(args.ToList());
            }
            catch (NullReferenceException)
            {
                if (MessageBoxManager.YesNoBoxError($"Loading section {model.SectionName} failed. Apply default settings?"))
                {
                    model.SetDefaultModelValues();
                }
                else
                    throw new Exception("Unable to load settings");
            }
        }
    }


    public interface IUploaderModels
    {
        string SectionName { get; }

        void UploadModels(List<JProperty> prop);

        void SetDefaultModelValues();

        void SaveConfigurationModels(JObject root);
    }

    public interface IBotAgentConfigPathHolder
    {
        string BotAgentPath { get; }

        string BotAgentConfigPath { get; }
    }
}
