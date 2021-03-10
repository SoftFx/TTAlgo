using Caliburn.Micro;
using Google.Protobuf.WellKnownTypes;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class Journal<T>
    {
        private int journalSize;
        private ObservableCircularList<T> items = new ObservableCircularList<T>();

        public Journal(int journalSize)
        {
            this.journalSize = journalSize;
        }

        public bool IsJournalFull { get { return items.Count >= journalSize; } }
        public ObservableCircularList<T> Records { get { return items; } }

        public virtual void Add(T item)
        {
            Execute.OnUIThread(() => Append(item));
        }

        public virtual void Add(List<T> items)
        {
            Execute.OnUIThread(() =>
            {
                foreach (var item in items)
                    Append(item);
            });
        }

        protected virtual void OnAppended(T item) { }
        protected virtual void OnRemoved(T item) { }

        protected virtual void Append(T item)
        {
            if (IsJournalFull)
            {
                var removed = items.Dequeue();
                OnRemoved(removed);
            }

            items.Add(item);
            OnAppended(item);
        }

        public virtual void Clear()
        {
            Execute.OnUIThread(() => items.Clear());
        }
    }

    internal class BaseJournalMessage
    {
        public BaseJournalMessage()
        {
            TimeKey = new TimeKey(DateTime.UtcNow, 0);
        }

        public BaseJournalMessage(Timestamp time)
        {
            TimeKey = new TimeKey(time);
        }

        public TimeKey TimeKey { get; set; }
        public string Message { get; set; }
        public JournalMessageType Type { get; set; }
        public override string ToString()
        {
            return Message;
        }

        internal static JournalMessageType Convert(PluginLogRecord.Types.LogSeverity severity)
        {
            switch (severity)
            {
                case PluginLogRecord.Types.LogSeverity.Info: return JournalMessageType.Info;
                case PluginLogRecord.Types.LogSeverity.Error: return JournalMessageType.Error;
                case PluginLogRecord.Types.LogSeverity.Custom: return JournalMessageType.Custom;
                case PluginLogRecord.Types.LogSeverity.Trade: return JournalMessageType.Trading;
                case PluginLogRecord.Types.LogSeverity.TradeSuccess: return JournalMessageType.TradingSuccess;
                case PluginLogRecord.Types.LogSeverity.TradeFail: return JournalMessageType.TradingFail;
                case PluginLogRecord.Types.LogSeverity.Alert: return JournalMessageType.Alert;
                default: return JournalMessageType.Info;
            }
        }
    }

    internal enum JournalMessageType
    {
        Info,
        Trading,
        TradingSuccess,
        TradingFail,
        Error,
        Custom,
        Alert,
    }
}
