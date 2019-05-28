using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotAgent.Configurator
{
    public class ProtocolManager
    {
        private const string SectionName = "Protocol";

        public ProtocolModel ProtocolModel { get; }

        public ProtocolManager()
        {
            ProtocolModel = new ProtocolModel();
        }

        public void UploadProtocolModel(List<JProperty> protocolProp)
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
                    default:
                        throw new Exception($"Unknown property {prop.Name}");
                }
            }
        }

        public void SaveProtocolModel(JObject obj)
        {
            obj[SectionName]["ListeningPort"] = ProtocolModel.ListeningPort.ToString();
            obj[SectionName]["LogDirectoryName"] = ProtocolModel.DirectoryName;
            obj[SectionName]["LogMessages"] = ProtocolModel.LogMessage.ToString();
        }
    }

    public class ProtocolModel
    {
        public int ListeningPort { get; set; }

        public string DirectoryName { get; set; }

        public bool LogMessage { get; set; }
    }
}
