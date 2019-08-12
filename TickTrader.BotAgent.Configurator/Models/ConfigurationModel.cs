using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TickTrader.BotAgent.Configurator
{
    public enum SectionNames {None, Credentials, Ssl, Protocol, Fdk, Server, MultipleAgentProvider }

    public class ConfigurationModel
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private ConfigManager _configManager;
        private PortsManager _portsManager;
        private RegistryManager _registryManager;

        private JObject _configurationObject;

        private readonly List<IUploaderModels> _uploaderModels;

        public ServiceManager ServiceManager { get; }

        public ConfigurationProperies Settings { get; }

        public CredentialsManager CredentialsManager { get; }

        public SslManager SslManager { get; }

        public ProtocolManager ProtocolManager { get; }

        public ServerManager ServerManager { get; }

        public FdkManager FdkManager { get; }

        public PrompterManager Prompts { get; }

        public LogsManager Logs { get; private set; }

        public MultipleAgentConfigurator BotAgentHolder { get; }

        public ConfigurationModel()
        {
            _configManager = new ConfigManager();

            Settings = _configManager.Properties;
            Prompts = new PrompterManager();

            BotAgentHolder = Settings.MultipleAgentProvider;
            _registryManager = new RegistryManager(Settings[AppProperties.RegistryAppName], Settings[AppProperties.AppSettings]);

            ServiceManager = new ServiceManager(Settings[AppProperties.ServiceName]);
            _portsManager = new PortsManager(ServiceManager);

            CredentialsManager = new CredentialsManager(SectionNames.Credentials);
            SslManager = new SslManager(SectionNames.Ssl);
            ProtocolManager = new ProtocolManager(SectionNames.Protocol, _portsManager);
            FdkManager = new FdkManager(SectionNames.Fdk);
            ServerManager = new ServerManager(_portsManager);

            _uploaderModels = new List<IUploaderModels>() { CredentialsManager, SslManager, ProtocolManager, ServerManager, FdkManager };

            LoadConfiguration(false);
        }

        public void StartAgent()
        {
            string ports = $"{string.Join(",", ServerManager.ServerModel.Urls.Select(u => u.Port.ToString()))},{ProtocolManager.ProtocolModel.ListeningPort}";

            _portsManager.RegisterRuleInFirewall(Settings[AppProperties.ApplicationName], Path.Combine(BotAgentHolder.BotAgentPath, $"{Settings[AppProperties.ApplicationName]}.exe"), ports, Settings[AppProperties.ServiceName]);

            if (ServiceManager.IsServiceRunning)
                 ServiceManager.ServiceStop();

             ServiceManager.ServiceStart(ProtocolManager.ProtocolModel.ListeningPort);
        }

        public void LoadConfiguration(bool loadConfig = true)
        {
            if (loadConfig)
                _configManager.LoadProperties();

            BotAgentHolder.SetNewBotAgentPath(_registryManager.BotAgentPath, Settings[AppProperties.AppSettings]);

            Logs = new LogsManager(BotAgentHolder.BotAgentPath, Settings[AppProperties.LogsPath]);

            using (var configStreamReader = new StreamReader(BotAgentHolder.BotAgentConfigPath))
            {
                _configurationObject = JObject.Parse(configStreamReader.ReadToEnd());

                foreach (var uploader in _uploaderModels)
                    UploadModel(uploader);
            }

            _configManager.SaveChanges();
        }

        public void SaveChanges()
        {
            foreach (var uploader in _uploaderModels)
                uploader.SaveConfigurationModels(_configurationObject);

            using (var configStreamWriter = new StreamWriter(BotAgentHolder.BotAgentConfigPath))
            {
                configStreamWriter.Write(_configurationObject.ToString());
            }

            _configManager.SaveChanges();
            UpdateCurrentModelValues();
        }

        private void UploadModel(IUploaderModels model)
        {
            try
            {
                var args = model.SectionName == "" ? _configurationObject.Values<JProperty>() : _configurationObject[model.SectionName].Children<JProperty>();
                model.UploadModels(args.ToList());
            }
            catch (NullReferenceException ex)
            {
                _logger.Error(ex);

                if (MessageBoxManager.YesNoBoxError($"Loading section {model.SectionName} failed. Apply default settings?"))
                {
                    model.SetDefaultModelValues();
                }
                else
                    throw new Exception("Unable to load settings");
            }
        }

        private void UpdateCurrentModelValues()
        {
            foreach (var model in _uploaderModels)
            {
                model.UpdateCurrentModelValues();
            }
        }
    }


    public interface IUploaderModels
    {
        string SectionName { get; }

        void UploadModels(List<JProperty> prop);

        void SetDefaultModelValues();

        void SaveConfigurationModels(JObject root);

        void UpdateCurrentModelValues();
    }

    public interface IBotAgentConfigPathHolder
    {
        string BotAgentPath { get; }

        string BotAgentConfigPath { get; }
    }
}
