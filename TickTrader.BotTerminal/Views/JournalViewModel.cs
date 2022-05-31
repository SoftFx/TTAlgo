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
using System.Collections.Specialized;
using TickTrader.WpfWindowsSupportLibrary;

namespace TickTrader.BotTerminal
{
    class JournalViewModel : PropertyChangedBase
    {
        private readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private EventJournal _eventJournal;

        private bool _isEnabled = true;

        public JournalViewModel(EventJournal journal)
        {
            _eventJournal = journal;

            Journal = CollectionViewSource.GetDefaultView(_eventJournal.Records);
            Journal.Filter = new Predicate<object>(Filter);
            Journal.SortDescriptions.Add(new SortDescription { PropertyName = "TimeKey", Direction = ListSortDirection.Descending });

            SearchModel = new SearchItemViewModel<EventMessage>(_eventJournal.Records, refresh: Journal.Refresh);
            Journal.CollectionChanged += SearchModel.CalculateMatches;
        }

        public SearchItemViewModel<EventMessage> SearchModel { get; }

        public ICollectionView Journal { get; private set; }

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
                WinExplorerHelper.ShowFolder(EnvService.Instance.JournalFolder);
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
