using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    public interface IListChangeNotifier<T>
    {
        event EventHandler<ListChangedEventArgs<T>> Changed;
    }

    public interface IReadonlyObservableList<T> : IReadOnlyList<T>, IListChangeNotifier<T>
    {
    }

    public interface IObservableList<T> : IList<T>, IListChangeNotifier<T>
    {
    }

    public class ListChangedEventArgs<T> : EventArgs
    {
        public ListChangedEventArgs(CollectionChangeActions action, T newItem, T oldItem, int index)
        {
            this.Action = action;
            this.NewItem = newItem;
            this.OldItem = oldItem;
            this.Index = index;
        }

        public CollectionChangeActions Action { get; private set; }
        public T NewItem { get; private set; }
        public T OldItem { get; private set; }
        public int Index { get; private set; }
        public bool HasNewItem { get { return Action == CollectionChangeActions.Added || Action == CollectionChangeActions.Replaced; } }
        public bool HasOldItem { get { return Action == CollectionChangeActions.Removed || Action == CollectionChangeActions.Replaced; } }
    }

    public enum CollectionChangeActions { Added, Removed, Replaced }
}
