using Caliburn.Micro;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;

namespace TickTrader.BotTerminal
{
    class JournalViewModel : PropertyChangedBase
    {
        private DispatcherTimer removeMeAfterUse;
        private EventJournal tradeJournal;
        private string filterString;
        private Logger logger;

        public JournalViewModel(EventJournal journal)
        {
            tradeJournal = journal;
            Journal = CollectionViewSource.GetDefaultView(tradeJournal.Events);
            Journal.Filter = new Predicate<object>(Filter);
            logger = NLog.LogManager.GetCurrentClassLogger();

            #region Remove Me
            removeMeAfterUse = new DispatcherTimer();
            removeMeAfterUse.Interval = TimeSpan.FromSeconds(4);
            removeMeAfterUse.Tick += (s, o) => FillFakeData();
            removeMeAfterUse.Start();
            #endregion
        }

        public ICollectionView Journal { get; private set; }
        public string FilterString
        {
            get { return filterString; }
            set
            {
                filterString = value;
                NotifyOfPropertyChange(nameof(FilterString));
                RefreshCollection();
            }
        }

        private void RefreshCollection()
        {
            if (this.Journal != null)
                Journal.Refresh();
        }

        public void Browse()
        {
            try
            {
                Directory.CreateDirectory(EnvService.Instance.JournalFolder);
                Process.Start(EnvService.Instance.JournalFolder);
            }
            catch (Exception ex)
            {
                logger.Warn(ex,"Failed to browse journal folder");
            }
        }

        public void Clear()
        {
            tradeJournal.Clear();
        }

        private bool Filter(object obj)
        {
            var data = obj as JournalItem;
            if (data != null)
            {
                if (!string.IsNullOrEmpty(filterString))
                    return data.Date.ToString("u").IndexOf(filterString, StringComparison.OrdinalIgnoreCase) >= 0 
                        || data.Message.IndexOf(filterString, StringComparison.OrdinalIgnoreCase) >= 0;
                return true;
            }
            return false;
        }

        private void FillFakeData()
        {
            string templateRequestSell = "\u2192 Request to Sell {0}k EURUSD is ACCEPTED, order OID{1} created";
            string templateRequestBuy = "\u2192 Request to Buy {0}k EURUSD is ACCEPTED, order OID{1} created";
            string templateOrder = "\u2192 Order OID{0} is FILLED at {1:0.####}, position PID{2}";

            var r = new Random();
            var oid = r.Next(100000, 1000000);
            var pid = r.Next(100000, 1000000);
            var price = 1 + r.NextDouble();
            var volume = r.Next(1, 1000);

            if (r.Next(1, 31) % 2 == 0)
                tradeJournal.Add(templateRequestSell, volume, oid);
            else
                tradeJournal.Add(templateRequestBuy, volume, oid);

            tradeJournal.Add(templateOrder, oid, price, pid);
        }
    }
}
