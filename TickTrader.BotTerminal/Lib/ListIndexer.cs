using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal.Lib
{
    public class ListIndexer<TKey, TVal>
    {
        private IList untypedList;
        private Func<TVal, TKey> indexFieldSelector;
        private SortedList<TKey, TVal> map = new SortedList<TKey, TVal>();

        public ListIndexer(IList listToIndex, Func<TVal, TKey> indexFieldSelector)
        {
            this.untypedList = listToIndex;
            this.indexFieldSelector = indexFieldSelector;
        }

        public IList Items { get { return untypedList; } }

        public void Add(TVal item)
        {
            TKey key = indexFieldSelector(item);
            map.Add(key, item);
            int index = map.IndexOfKey(key);
            untypedList.Insert(index, item);
        }

        public void Replace(TVal item)
        {
            TKey key = indexFieldSelector(item);
            int index = map.IndexOfKey(key);
            if (index < 0)
                throw new InvalidOperationException("No item to replace!");
            untypedList[index] = item;
        }

        public bool Remove(TKey key)
        {
            int index = map.IndexOfKey(key);
            if (index < 0)
                return false;
            map.RemoveAt(index);
            untypedList.RemoveAt(index);
            return true;
        }

        public bool ContainsKey(TKey key)
        {
            return map.ContainsKey(key);
        }
    }
}
