using Caliburn.Micro;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TickTrader.BotTerminal
{
    internal class Journal<T>
    {
        private int journalSize;
        private ObservableCircularList<T> items = new ObservableCircularList<T>();

        public Journal(int journalSize)
        {
            this.journalSize = journalSize;
        }

        public bool IsJournalFull { get { return items.Count >= journalSize; } }
        public ObservableCircularList<T> Records { get { return items; } }

        public virtual void Add(T item)
        {
            Execute.OnUIThread(() => Append(item));
        }

        public virtual void Add(List<T> items)
        {
            Execute.OnUIThread(() =>
            {
                foreach (var item in items)
                    Append(item);
            });
        }

        protected virtual void OnAppended(T item) { }
        protected virtual void OnRemoved(T item) { }

        protected virtual void Append(T item)
        {
            if (IsJournalFull)
            {
                var removed = items.Dequeue();
                OnRemoved(removed);
            }

            items.Add(item);
            OnAppended(item);
        }

        public void Clear()
        {
            Execute.OnUIThread(() => items.Clear());
        }
    }

    internal class BaseJournalMessage
    {
        public BaseJournalMessage()
        {
            Time = DateTime.UtcNow;
        }

        public BaseJournalMessage(DateTime time)
        {
            Time = time;
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
        Error,
        Custom
    }
}
