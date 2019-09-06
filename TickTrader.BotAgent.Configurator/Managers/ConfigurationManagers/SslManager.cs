using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TickTrader.BotAgent.Configurator.Properties;

namespace TickTrader.BotAgent.Configurator
{
    public class SslManager : ContentManager, IWorkingManager
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public SslModel SslModel { get; }

        public SslManager(SectionNames sectionName = SectionNames.None) : base(sectionName)
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
                }
            }

            SetDefaultModelValues();
        }

        public void SaveConfigurationModels(JObject root)
        {
            SaveProperty(root, "File", SslModel.File, SslModel.File, _logger);
            SaveProperty(root, "Password", SslModel.Password, SslModel.Password, _logger);
        }

        public void SetDefaultModelValues()
        {
            SslModel.SetDefaultValues();
        }

        public void UpdateCurrentModelValues()
        {
        }
    }
}
