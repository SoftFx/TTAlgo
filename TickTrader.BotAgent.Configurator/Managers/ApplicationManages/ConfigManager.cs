using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace TickTrader.BotAgent.Configurator
{
    public enum AppProperties { AppSettings, ApplicationName, RegistryAppName, ServiceName, LogsPath };

    public class ConfigManager
    {
        private readonly string _appConfigPath = Path.Combine(Environment.CurrentDirectory, "appConfig.json");

        private ConfigurationProperies _defaultProperties;

        public ConfigurationProperies Properties { get; }

        public ConfigManager()
        {
            Properties = new ConfigurationProperies();

            _defaultProperties = new ConfigurationProperies(new Dictionary<string, string>()
            {
                { "AppSettings", "WebAdmin/appsettings.json" },
                { "ApplicationName", "TickTrader.BotAgent" },
                { "RegistryAppName", "TickTrader Bot Agent" },
                { "ServiceName", "_sfxBotAgent" },
                { "LogsPath", "Logs/agent-error.log" }
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

            SaveChanges();
        }

        public void SaveChanges()
        {
            using (var fs = new FileStream(_appConfigPath, FileMode.Create))
            {
                using (var sw = new StreamWriter(fs))
                {
                    sw.Write(Properties.GetJObject().ToString());
                }
            }
        }
    }


    public class ConfigurationProperies
    {
        private Dictionary<string, string> _properties;

        public MultipleAgentConfigurator MultipleAgentProvider { get; private set; }

        public string this[string key] => _properties.ContainsKey(key) ? _properties[key] : null;

        public string this[AppProperties key] => this[key.ToString()];

        public ConfigurationProperies(Dictionary<string, string> properties = null)
        {
            _properties = properties ?? new Dictionary<string, string>();

            MultipleAgentProvider = new MultipleAgentConfigurator(SectionNames.MultipleAgentProvider);
        }

        public void LoadProperties(JObject obj)
        {
            foreach (string prop in Enum.GetNames(typeof(AppProperties)))
            {
                if (obj.SelectToken(prop) != null)
                    SetEmptyProperty(prop, obj[prop].ToString());
            }

            if (obj.SelectToken(nameof(MultipleAgentProvider)) != null)
                MultipleAgentProvider.LoadSettings(obj[nameof(MultipleAgentProvider)] as JObject, this[AppProperties.AppSettings]);
        }

        public JObject GetJObject()
        {
            var obj = JObject.FromObject(_properties);

            obj.Add(new JProperty(nameof(MultipleAgentProvider), MultipleAgentProvider.GetJObject()));

            return obj;
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

    public class MultipleAgentConfigurator : ContentManager, IBotAgentConfigPathHolder
    {
        public const string AgentCongPathsNameSection = "AgentConfigurationPaths";

        private int _selectPath = -1;
        private string _botAgentConfigPath;

        public string BotAgentPath
        {
            get => BotAgentPaths[SelectPath];
            set
            {
                if (BotAgentPath == value)
                    return;

                _selectPath = BotAgentPaths.IndexOf(value);
            }
        }

        public string BotAgentConfigPath => File.Exists(_botAgentConfigPath) ? _botAgentConfigPath : throw new Exception($"File not found {_botAgentConfigPath}");

        public int SelectPath
        {
            get
            {
                return (_selectPath >= -1 && _selectPath <= BotAgentPaths.Count) ?
                    _selectPath : throw new Exception($"Incorrect {nameof(SelectPath)} = {_selectPath}");
            }
            set
            {
                if (_selectPath == value)
                    return;

                _selectPath = value;
            }
        }

        public List<string> BotAgentPaths { get; private set; }

        public MultipleAgentConfigurator(SectionNames sectionName = SectionNames.None) : base(sectionName)
        {
            BotAgentPaths = new List<string>();
        }

        public void LoadSettings(JObject obj, string appSetting = "")
        {
            if (obj.SelectToken(nameof(SelectPath)) != null)
                SelectPath = obj[nameof(SelectPath)].ToObject<int>() - 1;

            if (obj.SelectToken(AgentCongPathsNameSection) != null)
                BotAgentPaths = obj[AgentCongPathsNameSection].ToObject<List<string>>();

            if (BotAgentPaths.Count > 0)
                _botAgentConfigPath = Path.Combine(BotAgentPaths[_selectPath], appSetting);
        }

        public void SetNewBotAgentPath(string path, string appSetting = "")
        {
            if (!BotAgentPaths.Contains(path))
            {
                BotAgentPaths.Add(path);

                if (BotAgentPaths.Count == 1)
                {
                    _selectPath = 0;
                    _botAgentConfigPath = Path.Combine(BotAgentPaths[_selectPath], appSetting);
                }
            }
        }

        public JObject GetJObject()
        {
            return new JObject()
            {
                new JProperty(nameof(SelectPath), SelectPath + 1),
                new JProperty(AgentCongPathsNameSection, BotAgentPaths)
            };
        }
    }
}
