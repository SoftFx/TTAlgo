using Caliburn.Micro;
using NLog;
using System;
using System.ComponentModel;
using System.Windows.Data;
using TickTrader.Algo.Domain;
using TickTrader.Algo.ServerControl;

namespace TickTrader.BotTerminal
{
    class BotJournalViewModel : PropertyChangedBase
    {
        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private AlgoBotViewModel _bot;
        private BotMessageFilter _botJournalFilter = new BotMessageFilter();


        public BotJournalViewModel(AlgoBotViewModel bot)
        {
            _bot = bot;

            Journal = CollectionViewSource.GetDefaultView(_bot.Model.Journal.Records);
            Journal.Filter = msg => _botJournalFilter.Filter((BotMessage)msg);
            Journal.SortDescriptions.Add(new SortDescription { PropertyName = "TimeKey", Direction = ListSortDirection.Descending });

            SearchModel = new SearchItemViewModel<BotMessage>(_bot.Model.Journal.Records, _botJournalFilter.Filter, ApplyFilter);
        }

        public SearchItemViewModel<BotMessage> SearchModel { get; }

        public ICollectionView Journal { get; private set; }

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

        public bool IsBotJournalEnabled
        {
            get { return _botJournalFilter.IsEnabled; }
            set
            {
                if (_botJournalFilter.IsEnabled != value)
                {
                    _botJournalFilter.IsEnabled = value;
                    NotifyOfPropertyChange(nameof(IsBotJournalEnabled));
                    ApplyFilter();
                }
            }
        }

        public bool CanBrowse => !_bot.Model.IsRemote || _bot.Agent.Model.AccessManager.CanGetBotFolderInfo(PluginFolderInfo.Types.PluginFolderId.BotLogs.ToApi());

        public bool IsRemote => _bot.Model.IsRemote;

        public void Clear()
        {
            _bot.Model.Journal.Clear();
        }

        public void Browse()
        {
            try
            {
                _bot.Browse();
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to browse bot journal folder");
            }
        }

        private void ApplyFilter()
        {
            if (Journal != null)
            {
                Journal.Filter = msg => _botJournalFilter.Filter((BotMessage)msg);

                SearchModel.FullCalculateMatches();
            }
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
        Custom,
        Alert,
    }

    internal class BotMessageFilter
    {
        public BotMessageFilter()
        {
            MessageTypeCondition = MessageTypeFilter.All;
            IsEnabled = true;
        }

        public string TextFilter { get; set; }
        public MessageTypeFilter MessageTypeCondition { get; set; }
        public bool IsEnabled { get; set; }

        public bool Filter(BotMessage bMessage) => IsEnabled && bMessage != null && MatchesTypeFilter(bMessage.Type);

        private bool MatchesTypeFilter(JournalMessageType journalType)
        {
            switch (MessageTypeCondition)
            {
                case MessageTypeFilter.All: return true;
                case MessageTypeFilter.Info: return journalType == JournalMessageType.Info;
                case MessageTypeFilter.Trading: return journalType == JournalMessageType.Trading || journalType == JournalMessageType.TradingSuccess || journalType == JournalMessageType.TradingFail;
                case MessageTypeFilter.Error: return journalType == JournalMessageType.Error;
                case MessageTypeFilter.Custom: return journalType == JournalMessageType.Custom;
                case MessageTypeFilter.Alert: return journalType == JournalMessageType.Alert;
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
