using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotTerminal
{
    public class ObservableCircularList<T> : CircularList<T>, INotifyCollectionChanged
    {
        public override void Add(T item)
        {
            base.Add(item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, Count - 1));
        }

        public override T Dequeue()
        {
            var item = base.Dequeue();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, 0));
            return item;
        }

        protected override void DoTruncateStart(int tSize)
        {
            if (tSize == Count) // Clear
            {
                base.DoTruncateStart(tSize);
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            else
            {
                var changedItems = this.Take(tSize).ToList();
                base.DoTruncateStart(tSize);
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, changedItems, 0));
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}
