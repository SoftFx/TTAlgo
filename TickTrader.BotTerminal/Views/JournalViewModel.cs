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
        private readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private EventJournal _eventJournal;
        private EventMessage _selectString;

        private string _filterString;

        private bool _isEnabled = true;

        public JournalViewModel(EventJournal journal)
        {
            _eventJournal = journal;
            Journal = CollectionViewSource.GetDefaultView(_eventJournal.Records);
            Journal.Filter = new Predicate<object>(Filter);
            Journal.SortDescriptions.Add(new SortDescription { PropertyName = "TimeKey", Direction = ListSortDirection.Descending });
        }

        public ICollectionView Journal { get; private set; }

        public EventMessage SelectString
        {
            get => _selectString;
            set
            {
                _selectString = value;
                NotifyOfPropertyChange(nameof(SelectString));
            }
        }

        public string FilterString
        {
            get { return _filterString; }
            set
            {
                _filterString = value;
                NotifyOfPropertyChange(nameof(FilterString));
                RefreshCollection();

                FindPreviosSubstring(_eventJournal.Records.Last(), true);
            }
        }

        public bool IsJournalEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
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
                logger.Warn(ex, "Failed to browse journal folder");
            }
        }

        public void Clear()
        {
            _eventJournal.Clear();
        }

        public void NextFoundMessage()
        {
            FindNextSubstring(SelectString);
        }

        public void PreviosFoundMessage()
        {
            FindPreviosSubstring(SelectString);
        }

        private void FindNextSubstring(EventMessage start, bool include = false)
        {
            int to = _eventJournal.Records.IndexOf(start);

            if (to == -1)
                return;

            for (int i = to + (include ? 0 : 1); i < _eventJournal.Records.Count; ++i)
            {
                var record = _eventJournal.Records[i];
                var index = record.Message.IndexOf(FilterString, StringComparison.CurrentCultureIgnoreCase);

                if (index != -1)
                {
                    SelectString = record;
                    break;
                }
            }
        }

        private void FindPreviosSubstring(EventMessage start, bool include = false)
        {
            int from = _eventJournal.Records.IndexOf(start);

            if (from == -1)
                return;

            for (int i = from - (include ? 0 : 1); i > -1; --i)
            {
                var record = _eventJournal.Records[i];
                var index = record.Message.IndexOf(FilterString, StringComparison.CurrentCultureIgnoreCase);

                if (index != -1)
                {
                    SelectString = record;
                    break;
                }
            }
        }

        private bool Filter(object obj) => _isEnabled;
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
