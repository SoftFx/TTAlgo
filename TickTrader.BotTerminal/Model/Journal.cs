using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TickTrader.BotTerminal
{
    internal class Journal<T>
    {
        private readonly int firstItem = 0;
        private int journalSize;
        private ObservableCollection<T> items;
        private object syncObj = new object();

        public Journal(int journalSize)
        {
            this.journalSize = journalSize;
            this.items = new ObservableCollection<T>();
            this.Events = new ReadOnlyObservableCollection<T>(items);
            BindingOperations.EnableCollectionSynchronization(Events, syncObj);
        }
        public bool IsJournalFull { get { return items.Count >= journalSize; } }
        public ReadOnlyObservableCollection<T> Events { get; private set; }

        public virtual void Add(T item)
        {
            lock (syncObj)
            {
                if (IsJournalFull)
                    items.RemoveAt(firstItem);

                items.Add(item);
            }
        }
        public void Clear()
        {
            lock (syncObj)
            {
                items.Clear();
            }
        }
    }

    internal class BaseJournalMessage
    {
        public BaseJournalMessage()
        {
            Time = DateTime.UtcNow;
        }

        public DateTime Time { get; set; }
        public string Message { get; set; }
        public JournalMessageType Type { get; set; }
        public override string ToString()
        {
            return Message;
        }
    }
    internal enum JournalMessageType
    {
        Info,
        Trading,
        Error
    }
}
