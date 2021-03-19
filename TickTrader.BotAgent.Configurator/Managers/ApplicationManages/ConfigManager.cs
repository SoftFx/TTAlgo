using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace TickTrader.BotAgent.Configurator
{
    public enum AppProperties { AppSettings, /*ApplicationName,*/ RegistryAppName, LogsPath, DeveloperVersion };

    public class ConfigManager
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly string _appConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appConfig.json");
        private readonly ConfigurationProperies _defaultProperties;

        public ConfigurationProperies Properties { get; }

        public ConfigManager()
        {
            Properties = new ConfigurationProperies();

            _defaultProperties = new ConfigurationProperies(new Dictionary<string, string>()
            {
                { "LogsPath", "Logs\\server.log" },
                { "DeveloperVersion", "false" }
            });

            LoadProperties();
        }

        public void LoadProperties()
        {
            if (File.Exists(_appConfigPath))
            {
                using (var sr = new StreamReader(_appConfigPath))
                {
                    string settings = sr.ReadToEnd();

                    if (!string.IsNullOrEmpty(settings))
                        Properties.LoadProperties(JObject.Parse(settings));
                }
            }

            Properties.Clone(_defaultProperties);

            CheckValues();
            SaveChanges();
        }

        public void SaveChanges()
        {
            try
            {
                using (var fs = new FileStream(_appConfigPath, FileMode.Create))
                {
                    using (var sw = new StreamWriter(fs))
                    {
                        sw.Write(Properties.GetJObject().ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private void CheckValues()
        {
            if (!bool.TryParse(Properties[AppProperties.DeveloperVersion], out bool status))
                Properties[AppProperties.DeveloperVersion] = "false";
        }
    }


    public class ConfigurationProperies
    {
        private const string AppSettings = "WebAdmin\\appsettings.json";
        //private const string ApplicationName = "TickTrader.AlgoServer";
        //private const string RegistryAppName = "TickTrader\\AlgoServer";

        private readonly Dictionary<string, string> _systemProperties;

        public readonly List<string> RegistryApplicationNames = new List<string> { "AlgoServer", "BotAgent" };

        public Dictionary<string, string> CustomProperties;

        public bool IsDeveloper => CustomProperties.ContainsKey(nameof(AppProperties.DeveloperVersion)) ? bool.Parse(this[AppProperties.DeveloperVersion]) : false;

        public string this[string key]
        {
            get
            {
                string value = null;

                if (CustomProperties.ContainsKey(key))
                    value = CustomProperties[key];

                if (_systemProperties.ContainsKey(key))
                    value = _systemProperties[key];

                return value;
            }
        }

        public string this[AppProperties key]
        {
            get => this[key.ToString()];

            set => CustomProperties[key.ToString()] = value;
        }

        public ConfigurationProperies(Dictionary<string, string> properties = null)
        {
            CustomProperties = properties ?? new Dictionary<string, string>();

            _systemProperties = new Dictionary<string, string>()
            {
                { nameof(AppProperties.AppSettings), AppSettings },
                //{ nameof(AppProperties.ApplicationName), ApplicationName },
                { nameof(AppProperties.RegistryAppName), RegistryApplicationNames[0] }
            };
        }

        public void LoadProperties(JObject obj)
        {
            foreach (string prop in Enum.GetNames(typeof(AppProperties)))
            {
                if (obj.SelectToken(prop) != null)
                    SetEmptyProperty(prop, obj[prop].ToString());
            }
        }

        public JObject GetJObject()
        {
            return JObject.FromObject(CustomProperties);
        }

        public void Clone(ConfigurationProperies defaultProp)
        {
            foreach (var prop in defaultProp.CustomProperties)
            {
                SetEmptyProperty(prop.Key, prop.Value);
            }
        }

        private void SetEmptyProperty(string key, string value)
        {
            if (!CustomProperties.ContainsKey(key))
                CustomProperties.Add(key, value);
            else
            if (string.IsNullOrEmpty(CustomProperties[key]))
                CustomProperties[key] = value;
        }
    }
}
