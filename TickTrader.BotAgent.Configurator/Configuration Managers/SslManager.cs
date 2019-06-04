using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotAgent.Configurator
{
    public class SslManager : ContentManager, IUploaderModels
    {
        public SslModel SslModel { get; }

        public SslManager(string sectioName = "") : base(sectioName)
        {
            SslModel = new SslModel();
        }

        public void UploadModels(List<JProperty> sslProp)
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

            SetDefaultModelValues();
        }

        public void SaveConfigurationModels(JObject root)
        {
            SaveProperty(root, "File", SslModel.File);
            SaveProperty(root, "Password", SslModel.Password);
        }

        public void SetDefaultModelValues()
        {
            SslModel.SetDefaultValues();
        }
    }

    public class SslModel
    {
        private const string DefaultFile = "certificate.ptx";

        public string File { get; set; }

        public string Password { get; set; }

        public void SetDefaultValues()
        {
            if (string.IsNullOrEmpty(File))
                File = DefaultFile;
        }
    }
}
