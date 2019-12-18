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

        private readonly SearchItemModel<EventMessage> _searchModel;

        private EventJournal _eventJournal;
        private EventMessage _selectItem;

        private string _filterString;

        private bool _isEnabled = true;

        public JournalViewModel(EventJournal journal)
        {
            _eventJournal = journal;
            _searchModel = new SearchItemModel<EventMessage>(_eventJournal.Records);

            Journal = CollectionViewSource.GetDefaultView(_eventJournal.Records);
            Journal.Filter = new Predicate<object>(Filter);
            Journal.SortDescriptions.Add(new SortDescription { PropertyName = "TimeKey", Direction = ListSortDirection.Descending });
        }

        public ICollectionView Journal { get; private set; }

        public EventMessage SelectItem
        {
            get => _selectItem;
            set
            {
                _selectItem = value;
                NotifyOfPropertyChange(nameof(SelectItem));
            }
        }

        public string FilterString
        {
            get => _filterString;
            set
            {
                _filterString = value;
                NotifyOfPropertyChange(nameof(FilterString));
                RefreshCollection();

                SelectItem = (EventMessage)_searchModel.FindPreviosSubstring(null, FilterString, true);
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

        public void FindNextItem()
        {
            SelectItem = (EventMessage)_searchModel.FindNextSubstring(SelectItem, FilterString);
        }

        public void FindPreviuosItem()
        {
            SelectItem = (EventMessage)_searchModel.FindPreviosSubstring(SelectItem, FilterString);
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
