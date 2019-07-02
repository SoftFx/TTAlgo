using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace TickTrader.BotAgent.Configurator
{
    public class FdkManager : ContentManager, IUploaderModels
    {
        public const string EnableLogsNameProperty = "EnableLogs";

        public FdkModel FdkModel { get; }

        public FdkManager(SectionNames sectionName = SectionNames.None) : base(sectionName)
        {
            FdkModel = new FdkModel();
        }

        public void UploadModels(List<JProperty> fdkProp)
        {
            foreach (var prop in fdkProp)
            {
                switch (prop.Name)
                {
                    case EnableLogsNameProperty:
                        FdkModel.EnableLogs = bool.Parse(prop.Value.ToString());
                        break;
                }
            }
        }

        public void SaveConfigurationModels(JObject root)
        {
            SaveProperty(root, EnableLogsNameProperty, FdkModel.EnableLogs);
        }

        public void SetDefaultModelValues() { }
    }

    public class FdkModel
    {
        public bool EnableLogs { get; set; }
    }
}
