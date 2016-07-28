using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    class BotJournalViewModel : PropertyChangedBase
    {
        private BotJournal botJournal;
        private BotMessageFilter botJournalFilter = new BotMessageFilter();
        private BotNameAggregator botNames = new BotNameAggregator();
        private ObservableCollection<BotNameFilterEntry> botNameFilterEntries = new ObservableCollection<BotNameFilterEntry>();

        public BotJournalViewModel(BotJournal journal)
        {
            botJournal = journal;

            Journal = CollectionViewSource.GetDefaultView(botJournal.Records.AsObservable());
            Journal.Filter = msg => botJournalFilter.Filter((BotMessage)msg);

            botNameFilterEntries.Add(new BotNameFilterEntry("All", true));

            botNames.Items.Updated += args =>
            {
                if (args.Action == DLinqAction.Insert)
                    botNameFilterEntries.Add(new BotNameFilterEntry(args.NewItem.Name, false));
                else if (args.Action == DLinqAction.Remove)
                {
                    var entry = botNameFilterEntries.FirstOrDefault((e) => !e.IsEmpty && e.Name == args.OldItem.Name);
                    if (entry != null)
                        botNameFilterEntries.Remove(entry);

                    if (selectedBotNameFilter == entry)
                        SelectedBotNameFilter = botNameFilterEntries[0];
                }
            };

            foreach (var item in journal.Records.Snapshot)
                botNames.Register(item);

            journal.Records.Updated += args =>
            {
                if (args.Action == DLinqAction.Insert)
                    botNames.Register(args.NewItem);
                else if (args.Action == DLinqAction.Remove)
                    botNames.UnRegister(args.OldItem);
            };

            SelectedBotNameFilter = botNameFilterEntries[0];
        }

        public ICollectionView Journal { get; private set; }
        public ObservableCollection<BotNameFilterEntry> BotNameFilterEntries { get { return botNameFilterEntries; } }
        public MessageTypeFilter TypeFilter
        {
            get { return botJournalFilter.MessageTypeFilter; }
            set
            {
                if (botJournalFilter.MessageTypeFilter != value)
                {
                    botJournalFilter.MessageTypeFilter = value;
                    NotifyOfPropertyChange(nameof(TypeFilter));
                    RefreshCollection();
                }
            }
        }
        public string TextFilter
        {
            get { return botJournalFilter.TextFilter; }
            set
            {
                if (botJournalFilter.TextFilter != value)
                {
                    botJournalFilter.TextFilter = value;
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
                    botJournalFilter.BotFilter = "";
                else
                    botJournalFilter.BotFilter = value.Name;

                selectedBotNameFilter = value;
                NotifyOfPropertyChange(nameof(SelectedBotNameFilter));
                RefreshCollection();
            }
        }

        public void Clear()
        {
            botJournal.Clear();
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
        Trading
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

    public class BotMsgCounter
    {
        public BotMsgCounter(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
        public int MessageCount { get; private set; }
        public bool HasMessages { get { return MessageCount > 0; } }
        public bool IsEmpty { get { return false; } }

        public void Increment()
        {
            MessageCount++;
        }

        public void Decrement()
        {
            MessageCount--;
        }
    }

    internal class BotNameAggregator
    {
        private DynamicDictionary<string, BotMsgCounter> botStats = new DynamicDictionary<string, BotMsgCounter>();

        public IDynamicDictionarySource<string, BotMsgCounter> Items { get { return botStats; } }

        public void Register(BotMessage msg)
        {
            BotMsgCounter item;
            if (!botStats.TryGetValue(msg.Bot, out item))
            {
                item = new BotMsgCounter(msg.Bot);
                botStats.Add(msg.Bot, item);
            }
            item.Increment();
        }

        public void UnRegister(BotMessage msg)
        {
            BotMsgCounter item;
            if (botStats.TryGetValue(msg.Bot, out item))
            {
                item.Decrement();
                if(!item.HasMessages)
                {
                    botStats.Remove(msg.Bot);
                }
            }
        }
    }
}
