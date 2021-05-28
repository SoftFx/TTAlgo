using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace TickTrader.BotTerminal
{
    [DataContract(Namespace = "BotTerminal.Profile.v2", Name = "ViewModelStorage")]
    internal class ViewModelStorageEntry
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<ViewModelPropertyStorageEntry> Properties { get; set; }


        public ViewModelStorageEntry()
        {
            Properties = new List<ViewModelPropertyStorageEntry>();
        }

        public ViewModelStorageEntry(string name) : this()
        {
            Name = name;
        }


        public ViewModelPropertyStorageEntry GetProperty(string key)
        {
            return Properties.Find(p => p.Key == key);
        }

        public void ChangeProperty(string key, string value)
        {
            var property = GetProperty(key);

            if (property != null)
                property.State = value;
            else
                Properties.Add(new ViewModelPropertyStorageEntry(key, value));
        }

        public ViewModelStorageEntry Clone()
        {
            return new ViewModelStorageEntry
            {
                Name = Name,
                Properties = Properties?.Select(p => p.Clone()).ToList(),
            };
        }
    }


    [DataContract(Namespace = "BotTerminal.Profile.v2", Name = "ViewModelProperty")]
    internal class ViewModelPropertyStorageEntry
    {
        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public string State { get; set; }


        public ViewModelPropertyStorageEntry() { }

        public ViewModelPropertyStorageEntry(string key, string value)
        {
            Key = key;
            State = value;
        }


        public ViewModelPropertyStorageEntry Clone()
        {
            return new ViewModelPropertyStorageEntry
            {
                Key = Key,
                State = State,
            };
        }
    }


    internal static class ViewModelStorageKeys
    {
        public const string GrossPositions = "GrossPositions";
        public const string GrossPositionsBacktester = "GrossPositionsBacktester";
        public const string NetPositions = "NetPositions";
        public const string NetPositionsBacktester = "NetPositionsBacktester";
        public const string Orders = "Orders";
        public const string OrdersBacktester = "OrdersBacktester";
        public const string History = "History";
        public const string HistoryBacktester = "HistoryBacktester";
    }
}
