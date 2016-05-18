using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    public interface IDynamicSet<T> : IEnumerable<T>, IDisposable
    {
        event SetUpdateHandler<T> Updated;
    }

    public interface IDynamicList<T> : IReadOnlyList<T>, IDisposable
    {
        event ListUpdateHandler<T> Updated;
    }

    public interface IDynamicDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IDisposable
    {
        event DictionaryUpdateHandler<TKey, TValue> Updated;
    }

    public interface IObservableList<T> : IReadOnlyList<T>, INotifyCollectionChanged, INotifyPropertyChanged, IDisposable
    {
    }

    public delegate void SetUpdateHandler<T>(SetUpdateArgs<T> args);
    public delegate void ListUpdateHandler<T>(ListUpdateArgs<T> args);
    public delegate void DictionaryUpdateHandler<TKey, TValue>(DictionaryUpdateArgs<TKey, TValue> args);

    public struct SetUpdateArgs<T>
    {
        public SetUpdateArgs(IDynamicSet<T> sender, DLinqUpdateType action, T newItem = default(T), T oldItem = default(T))
            : this()
        {
            this.Sender = sender;
            this.Action = action;
            this.NewItem = newItem;
            this.OldItem = oldItem;
        }

        public IDynamicSet<T> Sender { get; private set; }
        public DLinqUpdateType Action { get; private set; }
        public T NewItem { get; private set; }
        public T OldItem { get; private set; }
    }

    public struct ListUpdateArgs<T>
    {
        public ListUpdateArgs(IDynamicList<T> sender, DLinqUpdateType action, int index = -1, T newItem = default(T), T oldItem = default(T))
            : this()
        {
            this.Sender = sender;
            this.Action = action;
            this.Index = index;
            this.NewItem = newItem;
            this.OldItem = oldItem;
        }

        public IDynamicList<T> Sender { get; private set; }
        public DLinqUpdateType Action { get; private set; }
        public int Index { get; private set; }
        public T NewItem { get; private set; }
        public T OldItem { get; private set; }
    }

    public struct DictionaryUpdateArgs<TKey, TValue>
    {
        public DictionaryUpdateArgs(IDynamicDictionary<TKey, TValue> sender, DLinqUpdateType action,
            TKey key = default(TKey), TValue newItem = default(TValue), TValue oldItem = default(TValue)) : this()
        {
            this.Sender = sender;
            this.Action = action;
            this.Key = key;
            this.NewItem = newItem;
            this.OldItem = oldItem;
        }

        public IDynamicDictionary<TKey, TValue> Sender { get; private set; }
        public DLinqUpdateType Action { get; private set; }
        public TKey Key { get; private set; }
        public TValue NewItem { get; private set; }
        public TValue OldItem { get; private set; }
    }

    public enum DLinqUpdateType { Insert, Remove, Replace, Dispose, RemoveAll };
}
