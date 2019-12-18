using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    class BotJournalViewModel : PropertyChangedBase
    {
        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly SearchItemModel<BotMessage> _searchModel;

        private BotMessage _selectString;
        private AlgoBotViewModel _bot;
        private BotMessageFilter _botJournalFilter = new BotMessageFilter();


        public BotJournalViewModel(AlgoBotViewModel bot)
        {
            _bot = bot;

            _searchModel = new SearchItemModel<BotMessage>(_bot.Model.Journal.Records, _botJournalFilter.Filter);

            Journal = CollectionViewSource.GetDefaultView(_bot.Model.Journal.Records);
            Journal.Filter = msg => _botJournalFilter.Filter((BotMessage)msg);
            Journal.SortDescriptions.Add(new SortDescription { PropertyName = "TimeKey", Direction = ListSortDirection.Descending });
        }

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

        public BotMessage SelectItem
        {
            get => _selectString;
            set
            {
                _selectString = value;
                NotifyOfPropertyChange(nameof(SelectItem));
            }
        }

        public bool CanBrowse => !_bot.Model.IsRemote || _bot.Agent.Model.AccessManager.CanGetBotFolderInfo(BotFolderId.BotLogs);
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

        public void FindNextMessage()
        {
            SelectItem = (BotMessage)_searchModel.FindNextSubstring(SelectItem, TextFilter);
        }

        public void FindPreviousMessage()
        {
            SelectItem = (BotMessage)_searchModel.FindPreviosSubstring(SelectItem, TextFilter);
        }

        private void ApplyFilter()
        {
            if (Journal != null)
            {
                Journal.Filter = msg => _botJournalFilter.Filter((BotMessage)msg);

                SelectItem = (BotMessage)_searchModel.FindPreviosSubstring(null, TextFilter, true);
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
