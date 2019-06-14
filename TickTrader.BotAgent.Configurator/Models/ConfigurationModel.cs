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
        private IBotAgentConfigPathHolder _botAgentHolder;

        private DefaultServiceSettingsModel _defaultServiceSettings;

        private JObject _configurationObject;

        private readonly List<IUploaderModels> _uploaderModels;

        public ServiceManager ServiceManager { get; }

        public ConfigurationProperies Settings { get; }

        public CredentialsManager CredentialsManager { get; }

        public SslManager SslManager { get; }

        public ProtocolManager ProtocolManager { get; }

        public ServerManager ServerManager { get; }

        public FdkManager FdkManager { get; }


        public ConfigurationModel()
        {
            _defaultServiceSettings = new DefaultServiceSettingsModel();
            _configManager = new ConfigManager();
            Settings = _configManager.Properties;

            _botAgentHolder = Settings.UseProvider ? (IBotAgentConfigPathHolder)Settings.MultipleAgentProvider : new RegistryManager(Settings[AppProperties.RegistryAppName], Settings[AppProperties.AppSettings]);

            ServiceManager = new ServiceManager(Settings[AppProperties.ServiceName]);
            _portsManager = new PortsManager(ServiceManager, _defaultServiceSettings);

            CredentialsManager = new CredentialsManager("Credentials");
            SslManager = new SslManager("Ssl");
            ProtocolManager = new ProtocolManager("Protocol", _portsManager);
            FdkManager = new FdkManager("Fdk");
            ServerManager = new ServerManager();

            _uploaderModels = new List<IUploaderModels>() { CredentialsManager, SslManager, ProtocolManager, ServerManager, FdkManager };

            LoadConfiguration();
        }

        public bool StartAgent()
        {
            if (ServiceManager.IsServiceRunning && !MessageBoxManager.YesNoBoxQuestion("The process is already running, restart it?"))
                return false;

            //var hostAndPort = _portsManager.GetHostAndPort(ServerManager.ServerModel.Urls);
            //_portsManager.RegistryPortInFirewall(hostAndPort.Item2, _registryManager.ApplicationName);

            _portsManager.RegisterApplicationInFirewall(Settings[AppProperties.ApplicationName], _botAgentHolder.BotAgentPath);

            if (ServiceManager.IsServiceRunning)
                ServiceManager.ServiceStop();

            _portsManager.CheckPortOpen(ServerManager.ServerModel.Urls);
            ServiceManager.ServiceStart();

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

            UploadDefaultServiceModel();
        }

        public void SaveChanges()
        {
            foreach (var uploader in _uploaderModels)
                uploader.SaveConfigurationModels(_configurationObject);

            using (var configStreamWriter = new StreamWriter(_botAgentHolder.BotAgentConfigPath))
            {
                configStreamWriter.Write(_configurationObject.ToString());
            }

            UploadDefaultServiceModel();
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

        private void UploadDefaultServiceModel()
        {
            _defaultServiceSettings.ListeningPort = ProtocolManager.ProtocolModel.ListeningPort;
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
