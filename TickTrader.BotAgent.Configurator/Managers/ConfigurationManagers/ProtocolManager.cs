using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace TickTrader.BotAgent.Configurator
{
    public class ProtocolManager : ContentManager, IUploaderModels
    {
        public const string PortNameProperty = "ListeningPort";
        public const string DirectoryNameProperty = "LogDirectoryName";
        public const string UseLogNameProperty = "LogMessages";

        public ProtocolModel ProtocolModel { get; }

        public ProtocolManager(SectionNames sectionName = SectionNames.None, PortsManager manager = null) : base(sectionName)
        {
            ProtocolModel = new ProtocolModel(manager);
        }

        public void UploadModels(List<JProperty> protocolProp)
        {
            foreach (var prop in protocolProp)
            {
                switch (prop.Name)
                {
                    case PortNameProperty:
                        ProtocolModel.ListeningPort = int.Parse(prop.Value.ToString());
                        break;
                    case DirectoryNameProperty:
                        ProtocolModel.DirectoryName = prop.Value.ToString();
                        break;
                    case UseLogNameProperty:
                        ProtocolModel.LogMessage = bool.Parse(prop.Value.ToString());
                        break;
                }
            }

            SetDefaultModelValues();
        }

        public void SaveConfigurationModels(JObject root)
        {
            SaveProperty(root, PortNameProperty, ProtocolModel.ListeningPort);
            SaveProperty(root, DirectoryNameProperty, ProtocolModel.DirectoryName);
            SaveProperty(root, UseLogNameProperty, ProtocolModel.LogMessage);
        }

        public void SetDefaultModelValues()
        {
            ProtocolModel.SetDefaultValues();
        }
    }

    public class ProtocolModel
    {
        private const int MaxPort = 1 << 16;

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

        public void CheckPort(int port)
        {
            try
            {
                _portManager?.CheckPortOpen(port);
            }
            catch (WarningException ex)
            {
                string freePortMassage = string.Empty;

                for (int i = (port + 1) % MaxPort; i != port;)
                {
                    if (_portManager.CheckPortOpen(i, exception: false))
                    {
                        freePortMassage = $"Port {i} is free";
                        break;
                    }

                    i = (i + 1) % MaxPort;
                }

                if (string.IsNullOrEmpty(freePortMassage))
                    freePortMassage = "Free ports not found";

                Logger.Error(ex);
                throw new WarningException($"{ex.Message}. {freePortMassage}");
            }
        }
    }
}
