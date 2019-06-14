using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TickTrader.BotAgent.Configurator
{
    public class FdkManager : ContentManager, IUploaderModels
    {
        public FdkModel FdkModel { get; }

        public FdkManager(string sectionName = "") : base(sectionName)
        {
            FdkModel = new FdkModel();
        }

        public void UploadModels(List<JProperty> fdkProp)
        {
            foreach (var prop in fdkProp)
            {
                switch (prop.Name)
                {
                    case "EnableLogs":
                        FdkModel.EnableLogs = bool.Parse(prop.Value.ToString());
                        break;
                }
            }
        }

        public void SaveConfigurationModels(JObject root)
        {
            SaveProperty(root, "EnableLogs", FdkModel.EnableLogs.ToString());
        }

        public void SetDefaultModelValues() { }
    }

    public class FdkModel
    {
        public bool EnableLogs { get; set; }
    }
}
