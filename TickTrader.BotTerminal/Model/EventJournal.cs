using NLog;

namespace TickTrader.BotTerminal
{
    internal class EventJournal : Journal<EventMessage>
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public EventJournal() : this(500) { }
        public EventJournal(int journalSize) : base(journalSize) { }

        public void Info(string message)
        {
            Info(message, new object[0]);
        }
        public void Info(string message, params object[] args)
        {
            Add(new EventMessage(string.Format(message, args), JournalMessageType.Info));
        }

        public void Trading(string message)
        {
            Trading(message, new object[0]);
        }
        public void Trading(string message, params object[] args)
        {
            Add(new EventMessage(string.Format(message, args), JournalMessageType.Trading));
        }

        public void Error(string message)
        {
            Error(message, new object[0]);
        }
        public void Error(string message, params object[] args)
        {
            Add(new EventMessage(string.Format(message, args), JournalMessageType.Error));
        }

        public override void Add(EventMessage item)
        {
            base.Add(item);
            logger.Info(item.ToString());
        }
    }

    internal class EventMessage : BaseJournalMessage
    {
        public EventMessage(string message)
        {
            Message = message;
        }

        public EventMessage(string message, JournalMessageType type) : this(message)
        {
            Type = type;
        }
    }


}
