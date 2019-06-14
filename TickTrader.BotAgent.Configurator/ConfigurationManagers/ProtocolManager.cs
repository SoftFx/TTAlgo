using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace TickTrader.BotAgent.Configurator
{
    public class ProtocolManager : ContentManager, IUploaderModels
    {
        public ProtocolModel ProtocolModel { get; }

        public ProtocolManager(string sectionName = "", PortsManager manager = null) : base(sectionName)
        {
            ProtocolModel = new ProtocolModel(manager);
        }

        public void UploadModels(List<JProperty> protocolProp)
        {
            foreach (var prop in protocolProp)
            {
                switch (prop.Name)
                {
                    case "ListeningPort":
                        ProtocolModel.ListeningPort = int.Parse(prop.Value.ToString());
                        break;
                    case "LogDirectoryName":
                        ProtocolModel.DirectoryName = prop.Value.ToString();
                        break;
                    case "LogMessages":
                        ProtocolModel.LogMessage = bool.Parse(prop.Value.ToString());
                        break;
                }
            }

            SetDefaultModelValues();
        }

        public void SaveConfigurationModels(JObject root)
        {
            SaveProperty(root, "ListeningPort", ProtocolModel.ListeningPort.ToString());
            SaveProperty(root, "LogDirectoryName", ProtocolModel.DirectoryName);
            SaveProperty(root, "LogMessages", ProtocolModel.LogMessage.ToString());
        }

        public void SetDefaultModelValues()
        {
            ProtocolModel.SetDefaultValues();
        }
    }

    public class ProtocolModel
    {
        private readonly PortsManager _portManager;

        private const string DefaultDirectoryName = "Logs";
        private const int DefaultPort = 58443;

        public ProtocolModel(PortsManager manager)
        {
            _portManager = manager;
        }

        public int ListeningPort { get; set; }

        public string DirectoryName { get; set; }

        public bool LogMessage { get; set; }

        public void SetDefaultValues()
        {
            if (string.IsNullOrEmpty(DirectoryName))
                DirectoryName = DefaultDirectoryName;

            if (ListeningPort == 0)
                ListeningPort = DefaultPort;
        }

        public void CheckListeningPort()
        {
            _portManager?.CheckPortOpen(ListeningPort);
        }
    }
}
