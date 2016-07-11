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
    class BotJournalViewModel : PropertyChangedBase
    {
        private BotJournal algoJournal;
        private string textFilter;
        private MessageTypeFilter typeFilter;

        public BotJournalViewModel(BotJournal journal)
        {
            algoJournal = journal;
            Journal = CollectionViewSource.GetDefaultView(algoJournal.Events);
            Journal.Filter = new Predicate<object>(Filter);
            TypeFilter = MessageTypeFilter.All;
        }

        public ICollectionView Journal { get; private set; }
        public MessageTypeFilter TypeFilter
        {
            get { return typeFilter; }
            set
            {
                if (typeFilter != value)
                {
                    typeFilter = value;
                    NotifyOfPropertyChange(nameof(TypeFilter));
                    RefreshCollection();
                }
            }
        }
        public string TextFilter
        {
            get { return textFilter; }
            set
            {
                if (textFilter != value)
                {
                    textFilter = value;
                    NotifyOfPropertyChange(nameof(TextFilter));
                    RefreshCollection();
                }
            }
        }

        private void RefreshCollection()
        {
            if (this.Journal != null)
                Journal.Refresh();
        }


        public void Clear()
        {
            algoJournal.Clear();
        }

        private bool Filter(object obj)
        {
            var data = obj as BotMessage;
            if (data != null)
            {
                return (MessageType == null || data.Type == MessageType)
                     && (string.IsNullOrEmpty(textFilter) 
                     || (data.Time.ToString("dd/MM/yyyy HH:mm:ss.fff").IndexOf(textFilter, StringComparison.OrdinalIgnoreCase) >= 0
                     || data.Bot.IndexOf(textFilter, StringComparison.OrdinalIgnoreCase) >= 0
                     || data.Message.IndexOf(textFilter, StringComparison.OrdinalIgnoreCase) >= 0));
            }
            return false;
        }

        private JournalMessageType? MessageType
        {
            get
            {
                switch (TypeFilter)
                {
                    case MessageTypeFilter.Info: return JournalMessageType.Info;
                    case MessageTypeFilter.Trading: return JournalMessageType.Trading;
                    default: return null;
                }
            }
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
}
