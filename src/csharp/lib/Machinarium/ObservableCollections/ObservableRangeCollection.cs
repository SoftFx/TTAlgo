using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Machinarium.ObservableCollections
{
    public class ObservableRangeCollection<T> : ObservableCollection<T>
    {
        //stolen from ObservableCollection C# implementation 
        private const string CountString = "Count";
        private const string IndexerName = "Item[]";

        public ObservableRangeCollection() : base() { }

        public ObservableRangeCollection(IEnumerable<T> items) : base(items) { }


        public void AddRange(IEnumerable<T> newItems)
        {
            if (newItems == null)
                throw new ArgumentException($"Incorrect argument: {nameof(newItems)} is null");

            CheckReentrancy();

            foreach (var item in newItems)
                Items.Add(item);

            OnPropertyChanged(new PropertyChangedEventArgs(CountString));
            OnPropertyChanged(new PropertyChangedEventArgs(IndexerName));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
