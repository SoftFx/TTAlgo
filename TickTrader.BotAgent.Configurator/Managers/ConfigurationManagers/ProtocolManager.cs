using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace TickTrader.BotAgent.Configurator
{
    public class ProtocolManager : ContentManager, IWorkingManager
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
}
