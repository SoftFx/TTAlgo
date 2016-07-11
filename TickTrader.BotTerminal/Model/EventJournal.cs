using Caliburn.Micro;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TickTrader.BotTerminal
{
    internal class EventJournal
    {
        private readonly int firstItem = 0;
        private int journalSize;
        private ObservableCollection<JournalItem> items;
        private Logger logger;
        private object syncObj = new object();

        public EventJournal(int journalSize = 200)
        {
            this.journalSize = journalSize;
            this.items = new ObservableCollection<JournalItem>();
            this.Events = new ReadOnlyObservableCollection<JournalItem>(items);
            this.logger = NLog.LogManager.GetCurrentClassLogger();
            BindingOperations.EnableCollectionSynchronization(Events, syncObj);
        }

        private bool IsJournalFull { get { return items.Count >= journalSize; } }
        public ReadOnlyObservableCollection<JournalItem> Events { get; private set; }

        public void Add(string message)
        {
            Add(message, new object[0]);
        }

        public void Add(string message, params object[] args)
        {
            lock (syncObj)
            {
                if (IsJournalFull)
                    items.RemoveAt(firstItem);

                items.Add(new JournalItem(string.Format(message, args)));
            }
            logger.Info(message, args);
        }

        public void Clear()
        {
            lock (syncObj)
            {
                items.Clear();
            }
        }
    }

    internal class JournalItem
    {
        public JournalItem()
        {
            Date = DateTime.Now;
        }

        public JournalItem(string message) : this()
        {
            Message = message;
        }

        public DateTime Date { get; set; }
        public string Message { get; set; }
    }
}
