using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace TickTrader.BotAgent.Configurator
{
    public class AlgoServerSettingsManager : ContentManager, IWorkingManager
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public const string EnableDevModeProperty = "EnableDevMode";


        public AlgoServerSettingsModel AlgoServerModel { get; }


        public AlgoServerSettingsManager(SectionNames sectionName = SectionNames.None) : base(sectionName)
        {
            EnableManager = true; // for the first start, if appsettings.json not found
            AlgoServerModel = new AlgoServerSettingsModel();
        }


        public void UploadModels(List<JProperty> algoProp)
        {
            foreach (var prop in algoProp)
            {
                switch (prop.Name)
                {
                    case EnableDevModeProperty:
                        AlgoServerModel.EnableDevMode = bool.Parse(prop.Value.ToString());
                        break;
                }
            }

            UpdateCurrentModelValues();
        }

        public void SaveConfigurationModels(JObject root)
        {
            if (EnableManager)
                SaveProperty(root, EnableDevModeProperty, AlgoServerModel.EnableDevMode, AlgoServerModel.CurrentEnableDevModel, _logger);
        }

        public void SetDefaultModelValues() { }

        public void UpdateCurrentModelValues()
        {
            AlgoServerModel.UpdateCurrentFields();
        }
    }
}
