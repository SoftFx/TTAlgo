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
        private readonly string _cacheFile;

        private JObject _cacheObj = new JObject();

        public List<Uri> BusyUrls { get; private set; }


        public CacheManager(RegistryNode node)
        {
            BusyUrls = new List<Uri>();

            if (!Directory.Exists(_cashFolder))
                Directory.CreateDirectory(_cashFolder);

            _cacheFile = Path.Combine(_cashFolder, node.FullVersion);

            GetSettings();
        }

        public void RefreshCache(JObject cur)
        {
            SaveProperties(cur);
            GetSettings();
        }

        public void DropCash()
        {
            if (File.Exists(_cacheFile))
                File.Delete(_cacheFile);
        }

        public bool CompareJObject(JObject cur)
        {
            return JToken.DeepEquals(cur, _cacheObj);
        }

        public void SaveProperties(JObject obj)
        {
            try
            {
                using (var fs = new FileStream(_cacheFile, FileMode.Create))
                {
                    using (var sw = new StreamWriter(fs))
                    {
                        sw.Write(obj.ToString());
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
            if (_cacheObj.SelectToken(prop) == null)
                _cacheObj.Add(new JProperty(prop, new JObject()));

            _cacheObj[prop] = JToken.FromObject(value);
        }

        private void GetSettings()
        {
            if (File.Exists(_cacheFile))
            {
                try
                {
                    using (var sr = new StreamReader(_cacheFile))
                    {
                        _cacheObj = JObject.Parse(sr.ReadToEnd());
                    }

                    foreach (string prop in Enum.GetNames(typeof(CashedProperties)))
                    {
                        if (_cacheObj.SelectToken(prop) != null)
                            BusyUrls = (_cacheObj[prop] as JArray).Select(u => new Uri(u.ToString())).ToList();
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
