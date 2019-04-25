using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class ProviderColumnsState : ISettings
    {
        private List<ColumnStateStorageEntry> _source;
        private Dictionary<string, bool> _dict;
        private string _postfix;

        public object this[string key]
        {
            get => _dict[$"{key}_{_postfix}"];
            set
            {
                var fullKey = $"{key}_{_postfix}";
                _dict[fullKey] = (bool)value;
                var item = _source.FirstOrDefault(i => i.Key == fullKey);

                if (item == null)
                    _source.Add(new ColumnStateStorageEntry() { Key = fullKey, State = (bool)value, });
                else
                    item.State = (bool)value;
            }
        }

        public ProviderColumnsState(List<ColumnStateStorageEntry> source, string postfix = "")
        {
            _source = source;
            _postfix = postfix;

            _dict = _source?.ToDictionary(i => i.Key, i => i.State);
        }

        public bool ContainsKey(string key)
        {
            return _dict.ContainsKey($"{key}_{_postfix}");
        }
    }
}
