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
using TickTrader.Algo.Core;

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

            Journal = CollectionViewSource.GetDefaultView(_botJournal.Records);
            Journal.Filter = msg => _botJournalFilter.Filter((BotMessage)msg);
            Journal.SortDescriptions.Add(new SortDescription { PropertyName = "Time", Direction = ListSortDirection.Descending });

            _botNameFilterEntries.Add(new BotNameFilterEntry("Nothing", BotNameFilterType.Nothing));
            _botNameFilterEntries.Add(new BotNameFilterEntry("All", BotNameFilterType.All));

            _botJournal.Statistics.Items.Updated += args =>
            {
                if (args.Action == DLinqAction.Insert)
                    _botNameFilterEntries.Add(new BotNameFilterEntry(args.Key, BotNameFilterType.SpecifiedName));
                else if (args.Action == DLinqAction.Remove)
                {
                    var entry = _botNameFilterEntries.FirstOrDefault((e) => e.Type == BotNameFilterType.SpecifiedName && e.Name == args.Key);

                    if (selectedBotNameFilter == entry)
                        SelectedBotNameFilter = _botNameFilterEntries.First();

                    if (entry != null)
                        _botNameFilterEntries.Remove(entry);
                }
            };

            SelectedBotNameFilter = _botNameFilterEntries.First();
        }

        public ICollectionView Journal { get; private set; }
        public ObservableCollection<BotNameFilterEntry> BotNameFilterEntries { get { return _botNameFilterEntries; } }
        public MessageTypeFilter TypeFilter
        {
            get { return _botJournalFilter.MessageTypeCondition; }
            set
            {
                if (_botJournalFilter.MessageTypeCondition != value)
                {
                    _botJournalFilter.MessageTypeCondition = value;
                    NotifyOfPropertyChange(nameof(TypeFilter));
                    ApplyFilter();
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
                    ApplyFilter();
                }
            }
        }

        private BotNameFilterEntry selectedBotNameFilter;
        public BotNameFilterEntry SelectedBotNameFilter
        {
            get { return selectedBotNameFilter; }
            set
            {
                _botJournalFilter.BotCondition = value;

                selectedBotNameFilter = value;
                NotifyOfPropertyChange(nameof(SelectedBotNameFilter));
                ApplyFilter();
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
                string logDir;

                if (_botJournalFilter.BotCondition == null || _botJournalFilter.BotCondition.Type != BotNameFilterType.SpecifiedName)
                    logDir = EnvService.Instance.BotLogFolder;
                else
                    logDir = Path.Combine(EnvService.Instance.BotLogFolder, PathHelper.GetSafeFileName(_botJournalFilter.BotCondition.Name));

                Directory.CreateDirectory(logDir);
                Process.Start(logDir);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to browse bot journal folder");
            }
        }

        private void ApplyFilter()
        {
            if (this.Journal != null)
                Journal.Filter = msg => _botJournalFilter.Filter((BotMessage)msg);
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
            MessageTypeCondition = MessageTypeFilter.All;
        }

        public string TextFilter { get; set; }
        public MessageTypeFilter MessageTypeCondition { get; set; }
        public BotNameFilterEntry BotCondition { get; set; }

        public bool Filter(BotMessage bMessage)
        {
            if (bMessage != null)
            {
                return (BotCondition == null || BotCondition.Matches(bMessage.Bot))
                     && MatchesTypeFilter(bMessage.Type)
                     && (string.IsNullOrEmpty(TextFilter)
                     || (bMessage.Time.ToString(FullDateTimeConverter.Format).IndexOf(TextFilter, StringComparison.OrdinalIgnoreCase) >= 0
                     || bMessage.Bot.IndexOf(TextFilter, StringComparison.OrdinalIgnoreCase) >= 0
                     || bMessage.Message.IndexOf(TextFilter, StringComparison.OrdinalIgnoreCase) >= 0));
            }
            return false;
        }

        private bool MatchesTypeFilter(JournalMessageType journalType)
        {
            switch (MessageTypeCondition)
            {
                case MessageTypeFilter.All: return true;
                case MessageTypeFilter.Info: return journalType == JournalMessageType.Info;
                case MessageTypeFilter.Trading: return journalType == JournalMessageType.Trading || journalType == JournalMessageType.TradingSuccess || journalType == JournalMessageType.TradingFail;
                case MessageTypeFilter.Error: return journalType == JournalMessageType.Error;
                case MessageTypeFilter.Custom: return journalType == JournalMessageType.Custom;
                default: return true;
            }
        }
    }

    public class BotNameFilterEntry
    {
        public BotNameFilterEntry(string name, BotNameFilterType type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; private set; }
        public BotNameFilterType Type { get; private set; }

        public bool Matches(string botName)
        {
            if (Type == BotNameFilterType.Nothing)
                return false;
            else if (Type == BotNameFilterType.All)
                return true;
            else
                return Name == botName;
        }
    }

    public enum BotNameFilterType
    {
        Nothing,
        All,
        SpecifiedName
    }
}
