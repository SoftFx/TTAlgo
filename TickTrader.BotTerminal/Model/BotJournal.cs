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
            var botMessage = new BotMessage(botName, message, JournalMessageType.Custom);
            Add(botMessage);
            LogInfo(botMessage);
        }

        public void Custom(string botName, string message, params object[] args)
        {
            Custom(botName, string.Format(message, args));
        }

        public void Info(string botName, string message)
        {
            var botMessage = new BotMessage(botName, message, JournalMessageType.Info);
            Add(botMessage);
            LogInfo(botMessage);
        }

        public void Info(string botName, string message, params object[] args)
        {
            Info(botName, string.Format(message, args));
        }

        public void Error(string botName, string message)
        {
            var botMessage = new BotMessage(botName, message, JournalMessageType.Error);
            Add(botMessage);
            LogError(botMessage);
        }

        public void Error(string botName, string message, params object[] args)
        {
            Error(botName, string.Format(message, args));
        }

        public void Trading(string botName, string message)
        {
            var botMessage = new BotMessage(botName, message, JournalMessageType.Trading);
            Add(botMessage);
            LogInfo(botMessage);
        }

        public void Trading(string botName, string message, params object[] args)
        {
            Trading(botName, string.Format(message, args));
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
