using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TickTrader.BotTerminal.Infrustructure.Persistence
{
    [XmlRoot("Node")]
    public class SettingsPage
    {
        [XmlAttribute]
        public string Id { get; set; }

        [XmlIgnore]
        public Dictionary<string, SettingEntry> Settings { get; set; }

        [XmlArray("Settings")]
        [XmlArrayItem("S", typeof(FlatSettingEntry))]
        [XmlArrayItem("C", typeof(ComplexSettingEntry))]
        public List<SettingEntry> SettingsList { get; set; }

        [XmlElement("Node")]
        public List<SettingsPage> NestedPages { get; set; }

        public void OnSerializing()
        {
            if (Settings == null || Settings.Count == 0)
                SettingsList = null;
            else
                SettingsList = Settings.Values.ToList();
        }

        public void OnDeserializing()
        {
            if (SettingsList == null || SettingsList.Count == 0)
                Settings = null;
            else
                Settings = SettingsList.ToDictionary(s => s.Key);
        }

        public bool TryGetEntry(string settingId, out SettingEntry entry)
        {
            if (Settings == null)
            {
                entry = null;
                return false;
            }

            return Settings.TryGetValue(settingId, out entry);
        }

        public void Set(SettingEntry entry)
        {
            if (Settings == null)
                Settings = new Dictionary<string, SettingEntry>();

            Settings[entry.Key] = entry;
        }
    }

    [Serializable]
    public abstract class SettingEntry
    {
        [XmlAttribute]
        public string Key { get; set; }
    }

    [Serializable]
    public class FlatSettingEntry : SettingEntry
    {
        public FlatSettingEntry() { }

        public FlatSettingEntry(string key, string value)
        {
            Key = key;
            Val = value;
        }

        [XmlAttribute]
        public string Val { get; set; }
    }

    [Serializable]
    public class ComplexSettingEntry : SettingEntry
    {
        public ComplexSettingEntry() { }

        public ComplexSettingEntry(string key, object value)
        {
            Key = key;
            Val = value;
        }

        public object Val { get; set; }
    }
}
