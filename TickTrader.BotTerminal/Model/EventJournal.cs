using Google.Protobuf.WellKnownTypes;
using NLog;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class EventJournal : Journal<EventMessage>
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public EventJournal(int journalSize) : base(journalSize) { }

        public void Info(string message)
        {
            Add(new EventMessage(message, JournalMessageType.Info));
        }
        public void Info(string message, params object[] args)
        {
            Info(string.Format(message, args));
        }

        public void Trading(string message)
        {
            Add(new EventMessage(message, JournalMessageType.Trading));
        }
        public void Trading(string message, params object[] args)
        {
            Trading(string.Format(message, args));
        }

        public void Error(string message)
        {
            Add(new EventMessage(message, JournalMessageType.Error));
        }
        public void Error(string message, params object[] args)
        {
            Error(string.Format(message, args));
        }

        public override void Add(EventMessage item)
        {
            base.Add(item);
            logger.Info(item.ToString());
        }
    }

    internal class EventMessage : BaseJournalMessage
    {
        public EventMessage(string message, JournalMessageType type) : base()
        {
            Message = message;
            Type = type;
        }

        public EventMessage(Timestamp time, string message, JournalMessageType type) : base(time)
        {
            Message = message;
            Type = type;
        }

        public static EventMessage Create(PluginLogRecord record)
        {
            return new EventMessage(record.TimeUtc, record.Message, Convert(record.Severity));
        }
    }


}
