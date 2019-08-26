using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace TickTrader.BotAgent.Configurator
{
    public class FdkManager : ContentManager, IWorkingManager
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

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

            UpdateCurrentModelValues();
        }

        public void SaveConfigurationModels(JObject root)
        {
            SaveProperty(root, EnableLogsNameProperty, FdkModel.EnableLogs, FdkModel.CurrentEnableLogs, _logger);
        }

        public void SetDefaultModelValues() { }

        public void UpdateCurrentModelValues()
        {
            FdkModel.UpdateCurrentFields();
        }
    }
}
