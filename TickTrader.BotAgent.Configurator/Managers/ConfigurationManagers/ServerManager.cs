using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TickTrader.BotAgent.Configurator
{
    public class ServerManager : ContentManager, IUploaderModels
    {
        public const string UrlsNameProperty = "server.urls";
        public const string SecretKeyNameProperty = "SecretKey";

        public ServerModel ServerModel { get; }

        public ServerManager(SectionNames sectionName = SectionNames.None) : base(sectionName)
        {
            ServerModel = new ServerModel();
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
    }

    public class ServerModel
    {
        private const string DefaultUrls = "https://localhost:50000/";

        public List<Uri> Urls { get; private set; }

        public string UrlsStr
        {
            get => string.Join(";", Urls.Select(u => u.ToString()));
            set
            {
                if (UrlsStr == value)
                    return;

                Urls = value.Split(';').Select(u => new Uri(u)).ToList();
            }
        }

        public string SecretKey { get; set; }

        public ServerModel()
        {
            Urls = new List<Uri>();
        }

        public void SetDefaultValues()
        {
            if (string.IsNullOrEmpty(UrlsStr))
                UrlsStr = DefaultUrls;

            if (string.IsNullOrEmpty(SecretKey))
            {
                GenerateSecretKey();
            }
        }

        public void GenerateSecretKey()
        {
            SecretKey = CryptoManager.GetNewPassword(128);
        }
    }
}
