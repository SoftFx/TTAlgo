using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
    public interface IDynamicSetSource<T> : IDisposable
    {
        IEnumerable<T> Snapshot { get; }
        event SetUpdateHandler<T> Updated;
    }

    public interface IDynamicListSource<T> : IDisposable
    {
        IReadOnlyList<T> Snapshot { get; }
        event ListUpdateHandler<T> Updated;
    }

    public interface IDynamicDictionarySource<TKey, TValue> : IDisposable
    {
        IReadOnlyDictionary<TKey, TValue> Snapshot { get; }
        event DictionaryUpdateHandler<TKey, TValue> Updated;
    }

    public interface IDynamicDictionaryGrouping<TKey, TValue, TGroup> : IDynamicDictionarySource<TKey, TValue>
    {
        TGroup GroupKey { get; }
    }

    public interface IObservableListSource<T> : IReadOnlyList<T>, IList, INotifyCollectionChanged, INotifyPropertyChanged, IDisposable
    {
    }

    public interface IDynamicPropertySource<T> : IDisposable
    {
        T Value { get; }
        //event PropertyUpdateArgs<T> Updated 
    }

    public delegate void SetUpdateHandler<T>(SetUpdateArgs<T> args);
    public delegate void ListUpdateHandler<T>(ListUpdateArgs<T> args);
    public delegate void DictionaryUpdateHandler<TKey, TValue>(DictionaryUpdateArgs<TKey, TValue> args);

    public struct SetUpdateArgs<T>
    {
        public SetUpdateArgs(IDynamicSetSource<T> sender, DLinqAction action, T newItem = default(T), T oldItem = default(T))
            : this()
        {
            this.Sender = sender;
            this.Action = action;
            this.NewItem = newItem;
            this.OldItem = oldItem;
        }

        public IDynamicSetSource<T> Sender { get; private set; }
        public DLinqAction Action { get; private set; }
        public T NewItem { get; private set; }
        public T OldItem { get; private set; }
    }

    public struct ListUpdateArgs<T>
    {
        public ListUpdateArgs(IDynamicListSource<T> sender, DLinqAction action, int index = -1, T newItem = default(T), T oldItem = default(T))
            : this()
        {
            this.Sender = sender;
            this.Action = action;
            this.Index = index;
            this.NewItem = newItem;
            this.OldItem = oldItem;
        }

        public IDynamicListSource<T> Sender { get; private set; }
        public DLinqAction Action { get; private set; }
        public int Index { get; private set; }
        public T NewItem { get; private set; }
        public T OldItem { get; private set; }
    }

    public struct DictionaryUpdateArgs<TKey, TValue>
    {
        public DictionaryUpdateArgs(IDynamicDictionarySource<TKey, TValue> sender, DLinqAction action,
            TKey key = default(TKey), TValue newItem = default(TValue), TValue oldItem = default(TValue)) : this()
        {
            this.Sender = sender;
            this.Action = action;
            this.Key = key;
            this.NewItem = newItem;
            this.OldItem = oldItem;
        }

        public IDynamicDictionarySource<TKey, TValue> Sender { get; private set; }
        public DLinqAction Action { get; private set; }
        public TKey Key { get; private set; }
        public TValue NewItem { get; private set; }
        public TValue OldItem { get; private set; }
    }

    public enum DLinqAction { Insert, Remove, Replace, Dispose };
    public enum DPropertyAction { Update, Dispose }
}
