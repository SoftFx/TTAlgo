using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace TickTrader.BotAgent.Configurator
{
    public enum AppProperties { AppSettings, ApplicationName, RegistryAppName, LogsPath, DeveloperVersion };

    public class ConfigManager
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly string _appConfigPath = Path.Combine(Environment.CurrentDirectory, "appConfig.json");
        private readonly ConfigurationProperies _defaultProperties;

        public ConfigurationProperies Properties { get; }

        public ConfigManager()
        {
            Properties = new ConfigurationProperies();

            _defaultProperties = new ConfigurationProperies(new Dictionary<string, string>()
            {
                { "AppSettings", "WebAdmin\\appsettings.json" },
                { "ApplicationName", "TickTrader.BotAgent" },
                { "RegistryAppName", "TickTrader\\BotAgent" },
                { "LogsPath", "Logs\\agent.log" },
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
        private Dictionary<string, string> _properties;

        public string this[string key] => _properties.ContainsKey(key) ? _properties[key] : null;

        public string this[AppProperties key]
        {
            get => this[key.ToString()];

            set => _properties[key.ToString()] = value;
        }

        public ConfigurationProperies(Dictionary<string, string> properties = null)
        {
            _properties = properties ?? new Dictionary<string, string>();
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
            return JObject.FromObject(_properties);
        }

        public void Clone(ConfigurationProperies defaultProp)
        {
            foreach (string prop in Enum.GetNames(typeof(AppProperties)))
            {
                SetEmptyProperty(prop, defaultProp[prop]);
            }
        }

        private void SetEmptyProperty(string key, string value)
        {
            if (!_properties.ContainsKey(key))
                _properties.Add(key, value);
            else
            if (string.IsNullOrEmpty(_properties[key]))
                _properties[key] = value;
        }
    }
}
