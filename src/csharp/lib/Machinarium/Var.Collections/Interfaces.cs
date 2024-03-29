﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
    public interface IVarSet<T> : IDisposable
    {
        IEnumerable<T> Snapshot { get; }
        event SetUpdateHandler<T> Updated;
    }

    public interface IVarList<T> : IDisposable
    {
        IReadOnlyList<T> Snapshot { get; }
        event ListUpdateHandler<T> Updated;
    }

    public interface IVarSet<TKey, TValue> : IDisposable
    {
        IReadOnlyDictionary<TKey, TValue> Snapshot { get; }
        event DictionaryUpdateHandler<TKey, TValue> Updated;
    }

    public interface IVarGrouping<TKey, TValue, TGroup> : IVarSet<TKey, TValue>
    {
        TGroup GroupKey { get; }
    }

    public interface IObservableList<T> : IReadOnlyList<T>, IList, INotifyCollectionChanged, INotifyPropertyChanged, IDisposable
    {
    }

    public interface IVarProperty<T> : IDisposable
    {
        T Value { get; }
        //event PropertyUpdateArgs<T> Updated 
    }

    public delegate void SetUpdateHandler<T>(SetUpdateArgs<T> args);
    public delegate void ListUpdateHandler<T>(ListUpdateArgs<T> args);
    public delegate void DictionaryUpdateHandler<TKey, TValue>(DictionaryUpdateArgs<TKey, TValue> args);

    public struct SetUpdateArgs<T>
    {
        public SetUpdateArgs(IVarSet<T> sender, DLinqAction action, T newItem = default(T), T oldItem = default(T))
            : this()
        {
            this.Sender = sender;
            this.Action = action;
            this.NewItem = newItem;
            this.OldItem = oldItem;
        }

        public IVarSet<T> Sender { get; private set; }
        public DLinqAction Action { get; private set; }
        public T NewItem { get; private set; }
        public T OldItem { get; private set; }
    }

    public struct ListUpdateArgs<T>
    {
        public ListUpdateArgs(IVarList<T> sender, DLinqAction action, int index = -1, T newItem = default(T), T oldItem = default(T))
            : this()
        {
            this.Sender = sender;
            this.Action = action;
            this.Index = index;
            this.NewItem = newItem;
            this.OldItem = oldItem;
        }

        public IVarList<T> Sender { get; private set; }
        public DLinqAction Action { get; private set; }
        public int Index { get; private set; }
        public T NewItem { get; private set; }
        public T OldItem { get; private set; }
    }

    public readonly struct DictionaryUpdateArgs<TKey, TValue>
    {
        public DictionaryUpdateArgs(IVarSet<TKey, TValue> sender, DLinqAction action,
            TKey key = default, TValue newItem = default, TValue oldItem = default) : this()
        {
            Sender = sender;
            Action = action;
            Key = key;
            NewItem = newItem;
            OldItem = oldItem;
        }

        public IVarSet<TKey, TValue> Sender { get; }
        public DLinqAction Action { get; }
        public TKey Key { get; }
        public TValue NewItem { get; }
        public TValue OldItem { get; }
    }

    public enum DLinqAction { Insert, Remove, Replace, Dispose };
}
