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
        private JObject _configurationObject;

        public CredentialsManager CredentialsManager { get; }

        public SslManager SslManager { get; }

        public ProtocolManager ProtocolManager { get; }

        public ServerManager ServerManager { get; }

        private List<IUploaderModels> _uploaderModels;

        public ConfigurationModel()
        {
            _registryManager = new RegistryManager();

            CredentialsManager = new CredentialsManager("Credentials");
            SslManager = new SslManager("Ssl");
            ProtocolManager = new ProtocolManager("Protocol");
            ServerManager = new ServerManager();

            _uploaderModels = new List<IUploaderModels>() { CredentialsManager, SslManager, ProtocolManager, ServerManager };

            LoadConfiguration();
        }

        public void LoadConfiguration()
        {
            StreamReader configStreamReader = null;

            try
            {
                configStreamReader = _registryManager.GetConfigurationStreamReader();

                ParseJsonString(configStreamReader.ReadToEnd());
            }
            catch (Exception ex)
            {
                MessageBoxManager.ErrorBox(ex.Message);
                throw;
            }
            finally
            {
                configStreamReader?.Dispose();
            }
        }

        public void SaveChanges()
        {
            SslManager.SaveConfigurationModels(_configurationObject);
            ServerManager.SaveConfigurationModels(_configurationObject);
            ProtocolManager.SaveConfigurationModels(_configurationObject);
            CredentialsManager.SaveConfigurationModels(_configurationObject);

            SaveConfiguration();
        }

        private void SaveConfiguration()
        {
            StreamWriter configStreamWriter = null;
            bool successfully = false;

            try
            {
                configStreamWriter = _registryManager.GetConfigurationStreamWriter();
                configStreamWriter.Write(_configurationObject.ToString());

                successfully = true;
            }
            catch (Exception ex)
            {
                MessageBoxManager.ErrorBox(ex.Message);
            }
            finally
            {
                configStreamWriter?.Dispose();

                if (successfully)
                    MessageBoxManager.OKBox("Successfully");
            }
        }

        private void ParseJsonString(string json)
        {
            _configurationObject = JObject.Parse(json);

            UploadModel(CredentialsManager);
            UploadModel(SslManager);
            UploadModel(ProtocolManager);
            UploadModel(ServerManager);
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
