using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace TickTrader.BotAgent.Configurator
{
    public class ServerManager : ContentManager, IWorkingManager
    {
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
            SaveProperty(root, UrlsNameProperty, ServerModel.UrlsStr);
            SaveProperty(root, SecretKeyNameProperty, ServerModel.SecretKey);
        }

        public void SetDefaultModelValues()
        {
            ServerModel.SetDefaultValues();
        }

        public void UpdateCurrentModelValues()
        {
        }
    }
}
