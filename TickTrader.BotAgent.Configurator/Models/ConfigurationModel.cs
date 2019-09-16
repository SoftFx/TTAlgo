using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TickTrader.BotAgent.Configurator
{
    public enum SectionNames { None, Credentials, Ssl, Protocol, Fdk, Server, MultipleAgentProvider }

    public class ConfigurationModel
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private ConfigManager _configManager;
        private PortsManager _portsManager;
        private JObject _configurationObject;

        private List<IWorkingManager> _workingModels;

        public RegistryNode CurrentAgent => RegistryManager?.CurrentAgent;

        public RegistryManager RegistryManager { get; }

        public PrompterManager Prompts { get; }

        public ConfigurationProperies Settings { get; }

        public ServiceManager ServiceManager { get; private set; }

        public CredentialsManager CredentialsManager { get; private set; }

        public SslManager SslManager { get; private set; }

        public ProtocolManager ProtocolManager { get; private set; }

        public ServerManager ServerManager { get; private set; }

        public FdkManager FdkManager { get; private set; }

        public LogsManager Logs { get; private set; }

        public CacheManager CacheManager { get; private set; }

        public ConfigurationModel()
        {
            _configManager = new ConfigManager();

            Settings = _configManager.Properties;
            Prompts = new PrompterManager();
            RegistryManager = new RegistryManager(Settings[AppProperties.RegistryAppName], Settings[AppProperties.AppSettings], Settings.IsDeveloper, Settings[AppProperties.ApplicationName]);

            RefreshModel();
        }

        public void RefreshModel(string newPath = null)
        {
            RegistryManager.ChangeCurrentAgent(newPath);

            ServiceManager = new ServiceManager(CurrentAgent.ServiceId);
            CacheManager = new CacheManager(CurrentAgent);

            _portsManager = new PortsManager(RegistryManager.CurrentAgent, ServiceManager);
            _configurationObject = null;

            CredentialsManager = new CredentialsManager(SectionNames.Credentials);
            SslManager = new SslManager(SectionNames.Ssl);
            ProtocolManager = new ProtocolManager(SectionNames.Protocol, _portsManager);
            FdkManager = new FdkManager(SectionNames.Fdk);
            ServerManager = new ServerManager(_portsManager);
            Logs = new LogsManager(CurrentAgent.Path, Settings[AppProperties.LogsPath]);

            _workingModels = new List<IWorkingManager>() { CredentialsManager, SslManager, ProtocolManager, ServerManager, FdkManager };

            LoadConfiguration();
            SaveChanges();
        }

        public void StartAgent()
        {
            RegisterAgentInFirewall();
            ServiceManager.ServiceStart(ProtocolManager.ProtocolModel.ListeningPort.Value);
        }

        public void StopAgent()
        {
            ServiceManager.ServiceStop();
        }

        public void SaveCache()
        {
            //var ports = new List<Uri>(ServerManager.ServerModel.Urls) { ProtocolManager.ProtocolModel.ListeningUri };
            //CacheManager.SetProperty(CashedProperties.Ports, ports);
            CacheManager.RefreshCache(_configurationObject);
        }

        public void LoadConfiguration()
        {
            if (File.Exists(CurrentAgent.AppSettingPath))
                using (var configStreamReader = new StreamReader(CurrentAgent.AppSettingPath))
                {
                    _configurationObject = JObject.Parse(configStreamReader.ReadToEnd());
                }

            foreach (var model in _workingModels)
            {
                UploadModel(model);
                model.SetDefaultModelValues();
                model.UpdateCurrentModelValues();
            }
        }

        public void SaveChanges()
        {
            _configurationObject = _configurationObject ?? new JObject();

            foreach (var model in _workingModels)
            {
                model.SaveConfigurationModels(_configurationObject);
                model.UpdateCurrentModelValues();
            }

            using (var configStreamWriter = new StreamWriter(CurrentAgent.AppSettingPath))
            {
                configStreamWriter.Write(_configurationObject.ToString());
            }

            RegisterAgentInFirewall();
        }

        public bool EqualsCurrentAndCashedSettings()
        {
            return CacheManager.CompareJObject(_configurationObject);
        }

        private void UploadModel(IWorkingManager model)
        {
            if (_configurationObject == null)
                return;

            try
            {
                var args = model.SectionName == "" ? _configurationObject.Values<JProperty>() : _configurationObject[model.SectionName].Children<JProperty>();
                model.UploadModels(args.ToList());
            }
            catch (NullReferenceException ex)
            {
                _logger.Error(ex);
            }
        }

        private void RegisterAgentInFirewall()
        {
            string ports = $"{string.Join(",", ServerManager.ServerModel.Urls.Select(u => u.Port.ToString()))},{ProtocolManager.ProtocolModel.ListeningPort}";

            _portsManager.RegisterRuleInFirewall(Settings[AppProperties.ApplicationName], Path.Combine(CurrentAgent.Path, $"{Settings[AppProperties.ApplicationName]}.exe"), ports);
        }
    }

    public interface IWorkingManager
    {
        string SectionName { get; }

        void UploadModels(List<JProperty> prop);

        void SetDefaultModelValues();

        void SaveConfigurationModels(JObject root);

        void UpdateCurrentModelValues();
    }

    public interface IWorkingModel
    {
        void SetDefaultValues();
        void UpdateCurrentFields();
    }
}
