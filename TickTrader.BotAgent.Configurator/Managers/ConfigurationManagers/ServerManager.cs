using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace TickTrader.BotAgent.Configurator
{
    public class ServerManager : ContentManager, IWorkingManager
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public const string UrlsNameProperty = "server.urls";
        public const string SecretKeyNameProperty = "SecretKey";

        public ServerModel ServerModel { get; }

        public ServerManager(PortsManager manager, SectionNames sectionName = SectionNames.None) : base(sectionName)
        {
            ServerModel = new ServerModel(manager);
        }

        public void UploadModels(List<JProperty> serverProp)
        {
            foreach (var prop in serverProp)
            {
                switch (prop.Name)
                {
                    case UrlsNameProperty:
                        ServerModel.UrlsStr = prop.Value.ToString();
                        break;
                    case SecretKeyNameProperty:
                        ServerModel.SecretKey = prop.Value.ToString();
                        break;
                }
            }

            SetDefaultModelValues();
        }

        public void SaveConfigurationModels(JObject root)
        {
            SaveProperty(root, UrlsNameProperty, ServerModel.GetSortedUrls(), ServerModel.UrlsStr, _logger);
            SaveProperty(root, SecretKeyNameProperty, ServerModel.SecretKey, ServerModel.CurrentSecretKey, _logger, true);
        }

        public void SetDefaultModelValues()
        {
            ServerModel.SetDefaultValues();
        }

        public void UpdateCurrentModelValues()
        {
            ServerModel.UpdateCurrentFields();
        }
    }
}
