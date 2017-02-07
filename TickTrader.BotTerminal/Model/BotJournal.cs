using Machinarium.Qnil;
using NLog;
using System.Collections.Generic;
using System;

namespace TickTrader.BotTerminal
{
    internal class BotJournal : Journal<BotMessage>
    {
        private Dictionary<string, Logger> _loggers = new Dictionary<string, Logger>();

        public BotJournal() : this(500) { }
        public BotJournal(int journalSize) : base(journalSize)
        {
            Statistics = new BotNameAggregator();
            Statistics.Items.Updated += args =>
            {
                if (args.Action == DLinqAction.Remove)
                    _loggers.Remove(args.OldItem.Name);
            };

            Records.Updated += args =>
            {
                if (args.Action == DLinqAction.Insert)
                    Statistics.Register(args.NewItem);
                else if (args.Action == DLinqAction.Remove)
                    Statistics.UnRegister(args.OldItem);
            };
        }
        
        public BotNameAggregator Statistics { get; private set; }

        public void Custom(string botName, string message)
        {
            Custom(botName, message, new object[0]);
        }
        public void Custom(string botName, string message, params object[] args)
        {
            var botMessage = new BotMessage(botName, string.Format(message, args), JournalMessageType.Custom);
            Add(botMessage);
            LogInfo(botMessage);
        }
        public void Info(string botName, string message)
        {
            Info(botName, message, new object[0]);
        }
        public void Info(string botName, string message, params object[] args)
        {
            var botMessage = new BotMessage(botName, string.Format(message, args), JournalMessageType.Info);
            Add(botMessage);
            LogInfo(botMessage);
        }
        public void Error(string botName, string message)
        {
            Error(botName, message, new object[0]);
        }
        public void Error(string botName, string message, params object[] args)
        {
            var botMessage = new BotMessage(botName, string.Format(message, args), JournalMessageType.Error);
            Add(botMessage);
            LogError(botMessage);
        }
        public void Trading(string botName, string message)
        {
            Trading(botName, message, new object[0]);
        }
        public void Trading(string botName, string message, params object[] args)
        {
            var botMessage = new BotMessage(botName, string.Format(message, args), JournalMessageType.Trading);
            Add(botMessage);
            LogInfo(botMessage);
        }


        private void LogInfo(BotMessage message)
        {
            var logger = GetOrAddLogger(message.Bot);
            logger.Info(message.ToString());
        }
        private void LogError(BotMessage message)
        {
            var logger = GetOrAddLogger(message.Bot);
            logger.Error(message.ToString());
        }
        private Logger GetOrAddLogger(string botName)
        {
            var loggerName = $"{nameof(BotJournal)}.{PathHelper.GetSafeFileName(botName)}";
            return _loggers.GetOrAdd(loggerName, () => LogManager.GetLogger(loggerName));
        }
    }

    internal class BotMessage : BaseJournalMessage
    {
        public BotMessage(string botName, string message)
        {
            Message = message;
            Bot = botName;
        }

        public BotMessage(string botName, string message, JournalMessageType type) : this(botName, message)
        {
            Type = type;
        }

        public string Bot { get; set; }

        public override string ToString()
        {
            return $"{Type} | {Message}";
        }
    }
}
