using System;
using System.Collections.Generic;
using System.Linq;

namespace TickTrader.BotAgent.Configurator
{
    public class ServerModel : IWorkingModel
    {
        private const string DefaultUrls = "https://localhost:5001/;http://localhost:5000";

        private string _urlsString;

        public List<Uri> Urls { get; private set; }

        public PortsManager PortsManager { get; }

        public ServerModel(PortsManager portsManager)
        {
            PortsManager = portsManager;

            Urls = new List<Uri>();
        }

        public string UrlsStr
        {
            get => _urlsString;
            set
            {
                if (UrlsStr == value)
                    return;

                _urlsString = value;
                Urls = value.Split(';').Select(u => new Uri(u)).ToList();
            }
        }

        public string SecretKey { get; set; }

        public void SetDefaultValues()
        {
            UrlsStr = UrlsStr ?? DefaultUrls;

            if (SecretKey == null)
                GenerateSecretKey();
        }

        public void GenerateSecretKey()
        {
            SecretKey = CryptoManager.GetNewPassword(128);
        }

        public void UpdateCurrentFields() { }
    }
}
