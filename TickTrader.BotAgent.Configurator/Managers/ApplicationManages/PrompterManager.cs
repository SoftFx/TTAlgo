using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace TickTrader.BotAgent.Configurator
{
    public class PrompterManager
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly string _appPrompterFile = Path.Combine(Environment.CurrentDirectory, "prompterFile.json");

        private Dictionary<SectionNames, Dictionary<string, string>> _prompts { get; }

        public PrompterManager()
        {
            _prompts = new Dictionary<SectionNames, Dictionary<string, string>>();

            DownloadPrompts();
        }

        public Dictionary<string, string> GetDict(SectionNames section)
        {
            return _prompts.ContainsKey(section) ? _prompts[section] : null;
        }

        public string GetPrompt(SectionNames section, string key)
        {
            return _prompts.ContainsKey(section) && _prompts[section].ContainsKey(key) ? _prompts[section][key] : null;
        }

        private void DownloadPrompts()
        {
            JObject obj = null;

            try
            {
                using (var sr = new StreamReader(_appPrompterFile))
                {
                    string str = sr.ReadToEnd();

                    if (!string.IsNullOrEmpty(str))
                        obj = JObject.Parse(str);
                }

                foreach (SectionNames section in Enum.GetValues(typeof(SectionNames)))
                    if (section != SectionNames.None)
                    {
                        if (!_prompts.ContainsKey(section))
                            _prompts.Add(section, new Dictionary<string, string>());

                        string name = section.ToString();

                        foreach (var prop in obj[name].Children<JProperty>())
                        {
                            SetNewPrompts(section, prop.Name, prop.Value.ToString());
                        }
                    }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private void SetNewPrompts(SectionNames section, string key, string value)
        {
            if (!_prompts[section].ContainsKey(key))
                _prompts[section].Add(key, value);
            else
                _prompts[section][key] = value;
        }
    }
}
