using System.Collections.Generic;
using System.Runtime.Serialization;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    [DataContract(Namespace = "BotTerminal.Profile.v2", Name = "Plugin")]
    internal abstract class PluginStorageEntry<T> where T : PluginStorageEntry<T>, new()
    {
        [DataMember]
        public PluginConfig Config { get; set; }

        public virtual T Clone()
        {
            return new T
            {
                Config = Config,
            };
        }
    }


    [DataContract(Namespace = "BotTerminal.Profile.v2", Name = "Indicator")]
    internal class IndicatorStorageEntry : PluginStorageEntry<IndicatorStorageEntry>
    {
    }


    [DataContract(Namespace = "BotTerminal.Profile.v2", Name = "TradeBot")]
    internal class TradeBotStorageEntry : PluginStorageEntry<TradeBotStorageEntry>
    {
        [DataMember]
        public bool Started { get; set; }


        public override TradeBotStorageEntry Clone()
        {
            var res = base.Clone();
            res.Started = Started;
            return res;
        }
    }


    [DataContract(Namespace = "BotTerminal.Profile.v2", Name = "ViewModelProperties")]
    internal class ViewModelPropertiesStorageEntry
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<ViewPropertyStorageEntry> Properties { get; set; }


        internal ViewModelPropertiesStorageEntry()
        {
            Properties = new List<ViewPropertyStorageEntry>();
        }

        internal ViewModelPropertiesStorageEntry(string name)
        {
            Name = name;
            Properties = new List<ViewPropertyStorageEntry>();
        }

        internal ViewPropertyStorageEntry GetProperty(string key)
        {
            return Properties.Find(p => p.Key == key);
        }

        internal void ChangeProperty(string key, string value)
        {
            var property = GetProperty(key);

            if (property != null)
                property.State = value;
            else
                Properties.Add(new ViewPropertyStorageEntry(key, value));
        }
    }


    [DataContract(Namespace = "BotTerminal.Profile.v2", Name = "ViewProperty")]
    internal class ViewPropertyStorageEntry
    {
        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public string State { get; set; }

        internal ViewPropertyStorageEntry() { }

        internal ViewPropertyStorageEntry(string key, string value)
        {
            Key = key;
            State = value;
        }
    }
}
