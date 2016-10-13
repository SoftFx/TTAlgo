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
        private readonly int firstItem = 0;
        private int journalSize;
        private DynamicList<T> items = new DynamicList<T>();

        public Journal(int journalSize)
        {
            this.journalSize = journalSize;
        }
        public bool IsJournalFull { get { return items.Count >= journalSize; } }
        public IDynamicListSource<T> Records { get { return items; } }

        public virtual void Add(T item)
        {
            Execute.OnUIThread(() =>
            {
                if (IsJournalFull)
                    items.RemoveAt(firstItem);

                items.Add(item);
            });
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
