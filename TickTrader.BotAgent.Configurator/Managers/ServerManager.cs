using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace TickTrader.BotAgent.Configurator
{
    public class ServerManager : ContentManager, IUploaderModels
    {
        public ServerModel ServerModel { get; }

        public ServerManager(string sectionName = "") : base(sectionName)
        {
            ServerModel = new ServerModel();
        }

        public void UploadModels(List<JProperty> serverProp)
        {
            foreach (var prop in serverProp)
            {
                switch (prop.Name)
                {
                    case "server.urls":
                        ServerModel.Urls = prop.Value.ToString();
                        break;
                    case "SecretKey":
                        ServerModel.SecretKey = prop.Value.ToString();
                        break;
                }
            }

            SetDefaultModelValues();
        }

        public void SaveConfigurationModels(JObject root)
        {
            SaveProperty(root, "server.urls", ServerModel.Urls);
            SaveProperty(root, "SecretKey", ServerModel.SecretKey);
        }

        public void SetDefaultModelValues()
        {
            ServerModel.SetDefaultValues();
        }
    }

    public class ServerModel
    {
        private const string DefaultUrls = "https://localhost:50000/";
        private const string DefaultSecretKey = "kQ17Dww5EOFWBtSMWXMWgdjWouXRMhKT0AMEHuFqFhpv5j8rCGNqArAAufAbfYkgsDYZX7mShsrl8TYRiugEKSEz1oVLkXFg3GUyydfpW1DTX8YJcZzHwhQPXYJ6iWyd";

        public string Urls { get; set; }

        public string SecretKey { get; set; }

        public void SetDefaultValues()
        {
            if (string.IsNullOrEmpty(Urls))
                Urls = DefaultUrls;

            if (string.IsNullOrEmpty(SecretKey))
                SecretKey = DefaultSecretKey;
        }
    }
}
