﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotAgent.Configurator
{
    public enum SectionNames { None, Credentials, Ssl, Protocol, Fdk, Server, MultipleAgentProvider, Algo }

    public class ConfigurationModel
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private PortsManager _portsManager;
        private JObject _configurationObject;

        private List<IWorkingManager> _workingModels;

        public RegistryNode CurrentAgent => RegistryManager?.CurrentServer;

        public RegistryManager RegistryManager { get; }

        public PrompterManager Prompts { get; }

        public ServiceManager ServiceManager { get; private set; }

        public CredentialsManager CredentialsManager { get; private set; }

        public SslManager SslManager { get; private set; }

        public ProtocolManager ProtocolManager { get; private set; }

        public ServerManager ServerManager { get; private set; }

        public FdkManager FdkManager { get; private set; }

        public LogsManager Logs { get; private set; }

        //public CacheManager CacheManager { get; private set; }

        public ServerBotSettingsManager BotSettingsManager { get; private set; }

        public AlgoServerSettingsManager AlgoServerManager { get; private set; }


        public ConfigurationModel()
        {
            Prompts = new PrompterManager();
            RegistryManager = new RegistryManager();
            BotSettingsManager = new ServerBotSettingsManager(RegistryManager);

            RefreshModel();
        }

        public void RefreshModel(string newPath = null)
        {
            RegistryManager.ChangeCurrentAgent(newPath);

            ServiceManager = new ServiceManager(CurrentAgent.ServiceId);
            //CacheManager = new CacheManager(CurrentAgent);

            _portsManager = new PortsManager(RegistryManager.CurrentServer, ServiceManager);
            _configurationObject = null;

            CredentialsManager = new CredentialsManager(SectionNames.Credentials);
            SslManager = new SslManager(SectionNames.Ssl);
            ProtocolManager = new ProtocolManager(SectionNames.Protocol, _portsManager);
            FdkManager = new FdkManager(SectionNames.Fdk);
            AlgoServerManager = new AlgoServerSettingsManager(SectionNames.Algo);
            ServerManager = new ServerManager(_portsManager);
            Logs = new LogsManager(CurrentAgent.LogsFilePath);

            _workingModels = new List<IWorkingManager>()
            {
                CredentialsManager,
                SslManager,
                ProtocolManager,
                ServerManager,
                FdkManager,
                AlgoServerManager,
            };

            LoadConfiguration();
            SaveChanges();
        }

        public async Task StartAgent()
        {
            RegisterAgentInFirewall();
            await ServiceManager.ServiceStart(ProtocolManager.ProtocolModel.ListeningPort.Value);
        }

        public void StopAgent()
        {
            ServiceManager.ServiceStop();
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

            var appSettingsDir = Path.GetDirectoryName(CurrentAgent.AppSettingPath);
            PathHelper.EnsureDirectoryCreated(appSettingsDir);
            using (var configStreamWriter = new StreamWriter(CurrentAgent.AppSettingPath))
            {
                configStreamWriter.Write(_configurationObject.ToString());
            }

            RegisterAgentInFirewall();
        }

        private void UploadModel(IWorkingManager model)
        {
            if (_configurationObject == null)
                return;

            if (!string.IsNullOrEmpty(model.SectionName) && !_configurationObject.ContainsKey(model.SectionName))
            {
                model.EnableManager = false;
                return;
            }

            try
            {
                var args = string.IsNullOrEmpty(model.SectionName) ?
                           _configurationObject.Values<JProperty>() :
                           _configurationObject[model.SectionName].Children<JProperty>();

                model.UploadModels(args.ToList());
                model.EnableManager = true;
            }
            catch (NullReferenceException ex)
            {
                _logger.Error(ex);
            }
        }

        private void RegisterAgentInFirewall()
        {
            string ports = $"{string.Join(",", ServerManager.ServerModel.Urls.Select(u => u.Port.ToString()))},{ProtocolManager.ProtocolModel.ListeningPort}";

            _portsManager.RegisterRuleInFirewall(ports);
        }
    }

    public interface IWorkingManager
    {
        string SectionName { get; }

        bool EnableManager { get; set; }

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
