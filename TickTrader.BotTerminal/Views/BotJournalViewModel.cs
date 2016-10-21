using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Data;

namespace TickTrader.BotTerminal
{
    class BotJournalViewModel : PropertyChangedBase
    {
        private BotJournal _botJournal;
        private BotMessageFilter _botJournalFilter = new BotMessageFilter();
        private ObservableCollection<BotNameFilterEntry> _botNameFilterEntries = new ObservableCollection<BotNameFilterEntry>();
        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public BotJournalViewModel(BotJournal journal)
        {
            _botJournal = journal;

            Journal = CollectionViewSource.GetDefaultView(_botJournal.Records.AsObservable());
            Journal.Filter = msg => _botJournalFilter.Filter((BotMessage)msg);

            _botNameFilterEntries.Add(new BotNameFilterEntry("All", true));

            _botJournal.Statistics.Items.Updated += args =>
            {
                if (args.Action == DLinqAction.Insert)
                    _botNameFilterEntries.Add(new BotNameFilterEntry(args.NewItem.Name, false));
                else if (args.Action == DLinqAction.Remove)
                {
                    var entry = _botNameFilterEntries.FirstOrDefault((e) => !e.IsEmpty && e.Name == args.OldItem.Name);
                    if (entry != null)
                        _botNameFilterEntries.Remove(entry);

                    if (selectedBotNameFilter == entry)
                        SelectedBotNameFilter = _botNameFilterEntries[0];
                }
            };

            SelectedBotNameFilter = _botNameFilterEntries[0];
        }

        public ICollectionView Journal { get; private set; }
        public ObservableCollection<BotNameFilterEntry> BotNameFilterEntries { get { return _botNameFilterEntries; } }
        public MessageTypeFilter TypeFilter
        {
            get { return _botJournalFilter.MessageTypeFilter; }
            set
            {
                if (_botJournalFilter.MessageTypeFilter != value)
                {
                    _botJournalFilter.MessageTypeFilter = value;
                    NotifyOfPropertyChange(nameof(TypeFilter));
                    RefreshCollection();
                }
            }
        }
        public string TextFilter
        {
            get { return _botJournalFilter.TextFilter; }
            set
            {
                if (_botJournalFilter.TextFilter != value)
                {
                    _botJournalFilter.TextFilter = value;
                    NotifyOfPropertyChange(nameof(TextFilter));
                    RefreshCollection();
                }
            }
        }

        private BotNameFilterEntry selectedBotNameFilter;
        public BotNameFilterEntry SelectedBotNameFilter
        {
            get { return selectedBotNameFilter; }
            set
            {
                if (value.IsEmpty)
                    _botJournalFilter.BotFilter = "";
                else
                    _botJournalFilter.BotFilter = value.Name;

                selectedBotNameFilter = value;
                NotifyOfPropertyChange(nameof(SelectedBotNameFilter));
                RefreshCollection();
            }
        }

        public void Clear()
        {
            _botJournal.Clear();
        }

        public void Browse()
        {
            try
            {
                var logDir = Path.Combine(EnvService.Instance.BotLogFolder, PathHelper.GetSafeFileName(_botJournalFilter.BotFilter));
                Directory.CreateDirectory(logDir);
                Process.Start(logDir);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to browse bot journal folder");
            }
        }

        private void RefreshCollection()
        {
            if (this.Journal != null)
                Journal.Refresh();
        }
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    internal enum MessageTypeFilter
    {
        [Description("All events")]
        All,
        Info,
        Trading,
        Error,
        Custom
    }

    internal class BotMessageFilter
    {
        public BotMessageFilter()
        {
            MessageTypeFilter = MessageTypeFilter.All;
        }

        public string TextFilter { get; set; }
        public MessageTypeFilter MessageTypeFilter { get; set; }
        public string BotFilter { get; set; }

        public bool Filter(BotMessage bMessage)
        {
            if (bMessage != null)
            {
                return (JournalType == null || bMessage.Type == JournalType)
                     && (string.IsNullOrEmpty(BotFilter) || bMessage.Bot == BotFilter)
                     && (string.IsNullOrEmpty(TextFilter)
                     || (bMessage.Time.ToString("dd/MM/yyyy HH:mm:ss.fff").IndexOf(TextFilter, StringComparison.OrdinalIgnoreCase) >= 0
                     || bMessage.Bot.IndexOf(TextFilter, StringComparison.OrdinalIgnoreCase) >= 0
                     || bMessage.Message.IndexOf(TextFilter, StringComparison.OrdinalIgnoreCase) >= 0));
            }
            return false;
        }

        private JournalMessageType? JournalType
        {
            get
            {
                switch (MessageTypeFilter)
                {
                    case MessageTypeFilter.Info: return JournalMessageType.Info;
                    case MessageTypeFilter.Trading: return JournalMessageType.Trading;
                    case MessageTypeFilter.Error: return JournalMessageType.Error;
                    case MessageTypeFilter.Custom: return JournalMessageType.Custom;
                    default: return null;
                }
            }
        }
    }

    public class BotNameFilterEntry
    {
        public BotNameFilterEntry(string name, bool isEmpty)
        {
            Name = name;
            IsEmpty = isEmpty;
        }

        public string Name { get; private set; }
        public bool IsEmpty { get; private set; }
    }
}
