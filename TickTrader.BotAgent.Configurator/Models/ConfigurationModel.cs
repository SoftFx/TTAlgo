using Newtonsoft.Json.Linq;
using System;
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

        public ConfigurationModel()
        {
            _registryManager = new RegistryManager();

            CredentialsManager = new CredentialsManager();
            SslManager = new SslManager();
            ProtocolManager = new ProtocolManager();
            ServerManager = new ServerManager();

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
            }
            finally
            {
                configStreamReader?.Dispose();
            }
        }

        public void SaveChanges()
        {
            SslManager.SaveSslModel(_configurationObject);
            ServerManager.SaveServerModel(_configurationObject);
            ProtocolManager.SaveProtocolModel(_configurationObject);
            CredentialsManager.SaveCredentialsModels(_configurationObject);

            SaveConfiguration();
        }

        private void SaveConfiguration()
        {
            StreamWriter configStreamWriter = null;

            try
            {
                configStreamWriter = _registryManager.GetConfigurationStreamWriter();
                configStreamWriter.Write(_configurationObject.ToString());

                MessageBoxManager.OKBox("Settings saved successfully");
            }
            catch (Exception ex)
            {
                MessageBoxManager.ErrorBox(ex.Message);
            }
            finally
            {
                configStreamWriter?.Dispose();
            }
        }

        private void ParseJsonString(string json)
        {
            _configurationObject = JObject.Parse(json);

            CredentialsManager.UploadCredentials(_configurationObject["Credentials"].Children().Select(t => t.ToObject<JProperty>()).ToList());
            SslManager.UploadSslModel(_configurationObject["Ssl"].Children().Select(t => t.ToObject<JProperty>()).ToList());
            ProtocolManager.UploadProtocolModel(_configurationObject["Protocol"].Children().Select(t => t.ToObject<JProperty>()).ToList());
            ServerManager.UploadServerModel(_configurationObject.Values<JProperty>().ToList());
        }
    }
}
