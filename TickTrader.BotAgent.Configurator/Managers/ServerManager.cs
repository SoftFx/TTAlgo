using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotAgent.Configurator
{
    public class ServerManager
    {
        public ServerModel ServerModel { get; }

        public ServerManager()
        {
            ServerModel = new ServerModel();
        }

        public void UploadServerModel(List<JProperty> serverProp)
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
                    //default:
                    //    throw new Exception($"Unknown property {prop.Name}");
                }
            }
        }

        public void SaveServerModel(JObject obj)
        {
            obj["server.urls"] = ServerModel.Urls;
            obj["SecretKey"] = ServerModel.SecretKey;
        }
    }

    public class ServerModel
    {
        public string Urls { get; set; }

        public string SecretKey { get; set; }
    }
}
