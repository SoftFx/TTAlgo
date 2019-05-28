using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotAgent.Configurator
{
    public class SslManager
    {
        private const string SectionName = "Ssl";

        public SslModel SslModel { get; }

        public SslManager()
        {
            SslModel = new SslModel();
        }

        public void UploadSslModel(List<JProperty> sslProp)
        {
            foreach (var prop in sslProp)
            {
                switch (prop.Name)
                {
                    case "File":
                        SslModel.File = prop.Value.ToString();
                        break;
                    case "Password":
                        SslModel.Password = prop.Value.ToString();
                        break;
                    default:
                        throw new Exception($"Unknown property {prop.Name}");
                }
            }
        }

        public void SaveSslModel(JObject obj)
        {
            obj[SectionName]["File"] = SslModel.File;
            obj[SectionName]["Password"] = SslModel.Password;
        }
    }

    public class SslModel
    {
        public string File { get; set; }

        public string Password { get; set; }
    }
}
