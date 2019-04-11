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
using Machinarium.Qnil;

namespace TickTrader.BotTerminal
{
    class JournalViewModel : PropertyChangedBase
    {
        private EventJournal eventJournal;
        private string filterString;
        private readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private bool isEnabled = true;

        public JournalViewModel(EventJournal journal)
        {
            eventJournal = journal;
            Journal = CollectionViewSource.GetDefaultView(eventJournal.Records);
            Journal.Filter = new Predicate<object>(Filter);
            Journal.SortDescriptions.Add(new SortDescription { PropertyName = "Time", Direction = ListSortDirection.Descending });
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

        public bool IsJournalEnabled
        {
            get => isEnabled;
            set
            {
                isEnabled = value;
                NotifyOfPropertyChange(nameof(IsJournalEnabled));
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
            eventJournal.Clear();
        }

        private bool Filter(object obj)
        {
            if (!isEnabled)
                return false;

            var data = obj as EventMessage;
            if (data != null)
            {
                if (!string.IsNullOrEmpty(filterString))
                    return data.TimeKey.Timestamp.ToString("G").IndexOf(filterString, StringComparison.OrdinalIgnoreCase) >= 0 
                        || data.Message.IndexOf(filterString, StringComparison.OrdinalIgnoreCase) >= 0;
                return true;
            }
            return false;
        }
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    internal enum Journ
    {
        [Description("All events")]
        All,
        Info,
        Trading,
        Error,
        Custom
    }
}
