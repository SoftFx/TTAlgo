using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class BotJournal : Journal<BotMessage>
    {
        public BotMessageTypeCounter MessageCount { get; }

        public BotJournal(string botId) : this(botId, 1000) { }

        public BotJournal(string botId, int journalSize)
            : base(journalSize)
        {
            MessageCount = new BotMessageTypeCounter();
        }

        protected override void OnAppended(BotMessage item)
        {
            MessageCount.Added(item);
        }

        protected override void OnRemoved(BotMessage item)
        {
            MessageCount.Removed(item);
        }

        public override void Clear()
        {
            MessageCount.Reset();
            base.Clear();
        }
    }

    internal class BotMessage : BaseJournalMessage
    {
        public BotMessage(Timestamp time, string botName, string message, JournalMessageType type) : base(time)
        {
            Type = type;
            Message = message;
            Bot = botName;
        }

        public string Bot { get; set; }
        public string Details { get; set; }

        public override string ToString()
        {
            return $"{Type} | {Message}";
        }

        public static BotMessage Create(PluginLogRecord record, string instanceId)
        {
            return new BotMessage(record.TimeUtc, instanceId, record.Message, Convert(record.Severity)) { Details = record.Details };
        }
    }

    internal class BotMessageTypeCounter
    {
        private Dictionary<JournalMessageType, int> _messagesCnt;

        public int this[JournalMessageType type] => _messagesCnt[type];

        public BotMessageTypeCounter()
        {
            _messagesCnt = new Dictionary<JournalMessageType, int>();
            foreach (JournalMessageType type in System.Enum.GetValues(typeof(JournalMessageType)))
            {
                _messagesCnt.Add(type, 0);
            }
        }

        public void Added(BotMessage msg)
        {
            _messagesCnt[msg.Type]++;
        }

        public void Removed(BotMessage msg)
        {
            _messagesCnt[msg.Type]--;
        }

        public void Reset()
        {
            foreach (JournalMessageType type in System.Enum.GetValues(typeof(JournalMessageType)))
            {
                _messagesCnt[type] = 0;
            }
        }
    }
}
