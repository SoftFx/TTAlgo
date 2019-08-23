using Machinarium.Qnil;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    public class FileHistory
    {
        private int _maxSize;
        private Dictionary<string, ObservableCollection<Entry>> _collectionsByContext = new Dictionary<string, ObservableCollection<Entry>>();
        private Property<ObservableCollection<Entry>> _selectedCollectionProperty = new Property<ObservableCollection<Entry>>();

        public Var<ObservableCollection<Entry>> Items => _selectedCollectionProperty.Var;

        public FileHistory(int maxHistorySize = 5)
        {
            _maxSize = maxHistorySize;
        }

        public void SetContext(string context)
        {
            ObservableCollection<Entry> collection;
            if (!_collectionsByContext.TryGetValue(context, out collection))
            {
                collection = new ObservableCollection<Entry>();
                _collectionsByContext.Add(context, collection);
            }

            _selectedCollectionProperty.Value = collection;
        }

        public void Add(string filePath, bool moveUp)
        {
            var newEntry = new Entry(filePath);
            var collection = _selectedCollectionProperty.Value;

            if (moveUp)
            {
                collection.Remove(newEntry);
                collection.Insert(0, newEntry);

                if (collection.Count > _maxSize)
                    collection.RemoveLast();
            }
            else
            {
                if (collection.Count < _maxSize && !collection.Contains(newEntry))
                    collection.Add(newEntry);
            }
        }

        public void Remove(Entry e)
        {
            var collection = _selectedCollectionProperty.Value;
            collection.Remove(e);
        }

        public class Entry
        {
            public Entry(string path)
            {
                FullPath = path;
                FileName = System.IO.Path.GetFileNameWithoutExtension(path);
            }

            public string FullPath { get; }
            public string FileName { get; }

            public override string ToString()
            {
                return FileName;
            }

            public override bool Equals(object obj)
            {
                var otherEntry = obj as Entry;
                if (otherEntry == null)
                    return false;
                return string.Compare(otherEntry.FullPath, FullPath, true) == 0;
            }

            public override int GetHashCode()
            {
                return FullPath.GetHashCode();
            }
        }
    }
}
