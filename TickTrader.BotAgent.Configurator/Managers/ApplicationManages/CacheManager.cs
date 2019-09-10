using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TickTrader.BotAgent.Configurator
{
    public enum CashedProperties { Ports };

    public class CacheManager
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly string _cashFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache");

        private JObject _cashObj = new JObject();
        private string _cashFile;

        public List<Uri> BusyUrls { get; private set; } 


        public CacheManager(RegistryNode node)
        {
            BusyUrls = new List<Uri>();

            if (!Directory.Exists(_cashFolder))
                Directory.CreateDirectory(_cashFolder);

            _cashFile = Path.Combine(_cashFolder, node.FullVersion);

            GetSettings();
        }

        public void DropCash()
        {
            if (File.Exists(_cashFile))
                File.Delete(_cashFile);
        }

        public void SaveProperties()
        {
            try
            {
                using (var fs = new FileStream(_cashFile, FileMode.Create))
                {
                    using (var sw = new StreamWriter(fs))
                    {
                        sw.Write(_cashObj.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        public void SetProperty(CashedProperties prop, object value)
        {
            SetProperty(prop.ToString(), value);
        }

        public void SetProperty(string prop, object value)
        {
            if (_cashObj.SelectToken(prop) == null)
                _cashObj.Add(new JProperty(prop, new JObject()));

            _cashObj[prop] = JToken.FromObject(value);
        }

        private void GetSettings()
        {
            if (File.Exists(_cashFile))
            {
                try
                {
                    using (var sr = new StreamReader(_cashFile))
                    {
                        _cashObj = JObject.Parse(sr.ReadToEnd());
                    }

                    foreach (string prop in Enum.GetNames(typeof(CashedProperties)))
                    {
                        if (_cashObj.SelectToken(prop) != null)
                            BusyUrls = (_cashObj[prop] as JArray).Select(u => new Uri(u.ToString())).ToList();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }
        }
    }
}
