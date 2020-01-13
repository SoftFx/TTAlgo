using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    public class ObservableCounter<T> : IEnumerable<T>
    {
        private readonly T DefaultValue;
        
        private Dictionary<T, int> _count = new Dictionary<T, int>();

        //If implemented INotifyCollectionChanged NotifyCollectionChangedAction.Remove sometimes gives unexpected behavior therefore we use ObservableCollection
        public ObservableCollection<T> ExistingItems { get; } = new ObservableCollection<T>();

        public ObservableCounter(T defaultValue)
        {
            DefaultValue = defaultValue;
            Add(defaultValue);
        }   

        public void Clear()
        {
            _count.Clear();

            ExistingItems.Clear();
        }

        public void Remove(T item)
        {
            if (item.Equals(DefaultValue) || !_count.ContainsKey(item) || _count[item] <= 0)
                return;

            if (--_count[item] == 0)
                ExistingItems.Remove(item);
        }

        public void Add(T item)
        {
            if (!_count.ContainsKey(item))
                _count.Add(item, 0);

            if (++_count[item] == 1)
                ExistingItems.Add(item);
        }

        public IEnumerator<T> GetEnumerator() => ExistingItems.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
