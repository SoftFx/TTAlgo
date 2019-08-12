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
            UpdateCurrentModelValues();
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

        public void UpdateCurrentModelValues()
        {
            ProtocolModel.UpdateCurrentFields();
        }
    }

    public class ProtocolModel
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly PortsManager _portManager;

        private const string DefaultDirectoryName = "Logs";
        private const int DefaultPort = 58443;

        public ProtocolModel(PortsManager manager)
        {
            _portManager = manager;
        }

        public int ListeningPort { get; set; }

        public int CurrentListeningPort { get; set; }

        public string DirectoryName { get; set; }

        public string CurrentDirectoryName { get; set; }

        public bool LogMessage { get; set; }

        public bool CurrentLogMessage { get; set; }

        public void SetDefaultValues()
        {
            if (string.IsNullOrEmpty(DirectoryName))
                DirectoryName = DefaultDirectoryName;

            if (ListeningPort == 0)
                ListeningPort = DefaultPort;
        }

        public void UpdateCurrentFields()
        {
            CurrentDirectoryName = DirectoryName;
            CurrentListeningPort = ListeningPort;
            CurrentLogMessage = LogMessage;
        }

        public void CheckPort(int port)
        {
            _portManager.CheckPort(port);
        }
    }
}
