using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TickTrader.BotAgent.Configurator
{
    public class ConfigurationModel
    {
        private RegistryManager _registryManager;
        private PortsManager _portsManager;
        private JObject _configurationObject;

        private readonly List<IUploaderModels> _uploaderModels;

        public CredentialsManager CredentialsManager { get; }

        public SslManager SslManager { get; }

        public ProtocolManager ProtocolManager { get; }

        public ServerManager ServerManager { get; }


        public ConfigurationModel()
        {
            _registryManager = new RegistryManager();
            _portsManager = new PortsManager();

            CredentialsManager = new CredentialsManager("Credentials");
            SslManager = new SslManager("Ssl");
            ProtocolManager = new ProtocolManager("Protocol");
            ServerManager = new ServerManager();

            _uploaderModels = new List<IUploaderModels>() { CredentialsManager, SslManager, ProtocolManager, ServerManager };

            LoadConfiguration();
        }

        public void StartAgent()
        {
            var hostAndPort = _portsManager.GetHostAndPort(ServerManager.ServerModel.Urls);
            _portsManager.RegistryPortInFirewall(hostAndPort.Item2, _registryManager.ApplicationName);
            _portsManager.CheckPortOpen(ServerManager.ServerModel.Urls);
            MessageBoxManager.OKBox("Port is open");
        }

        public void LoadConfiguration()
        {
            using (var configStreamReader = _registryManager.GetConfigurationStreamReader())
            {
                ParseJsonString(configStreamReader.ReadToEnd());
            }
        }

        public void SaveChanges()
        {
            foreach (var uploader in _uploaderModels)
                uploader.SaveConfigurationModels(_configurationObject);

            SaveConfiguration();
        }

        private void SaveConfiguration()
        {
            using (var configStreamWriter = _registryManager.GetConfigurationStreamWriter())
            {
                configStreamWriter.Write(_configurationObject.ToString());
            }

            MessageBoxManager.OKBox("Successfully");
        }

        private void ParseJsonString(string json)
        {
            _configurationObject = JObject.Parse(json);

            foreach (var uploader in _uploaderModels)
                UploadModel(uploader);
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
                if (MessageBoxManager.YesNoBox($"Loading section {model.SectionName} failed. Apply default settings?"))
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
}
