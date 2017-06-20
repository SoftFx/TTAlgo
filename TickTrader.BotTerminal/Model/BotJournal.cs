using Machinarium.Qnil;
using NLog;
using System.Collections.Generic;
using System;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    internal class BotJournal : Journal<BotMessage>
    {
        private Dictionary<string, Logger> _loggers = new Dictionary<string, Logger>();
        private DynamicList<string> botNames = new DynamicList<string>();

        public BotJournal() : this(500) { }

        public IDynamicListSource<string> BotNames => botNames;

        public BotJournal(int journalSize) : base(journalSize)
        {
        }

        public override void Add(BotMessage item)
        {
            base.Add(item);

            WriteToLogger(item);
        }

        public override void Add(List<BotMessage> items)
        {
            base.Add(items);

            foreach (var item in items)
                WriteToLogger(item);
        }

        public void RegisterBotLog(string botName)
        {
            botNames.Add(botName);
        }

        public void UnregisterBotLog(string botName)
        {
            botNames.Remove(botName);
        }

        private void WriteToLogger(BotMessage message)
        {
            var logger = GetOrAddLogger(message.Bot);

            if (message.Type != JournalMessageType.Error)
                logger.Info(message.ToString());
            else
            {
                logger.Error(message.ToString());
                if (message.Details != null)
                    logger.Error(message.Details);
            }
        }

        private void LogError(BotMessage message)
        {
            var logger = GetOrAddLogger(message.Bot);
            logger.Error(message.ToString());
        }

        private Logger GetOrAddLogger(string botName)
        {
            var loggerName = LoggerHelper.GetBotLoggerName(botName);
            return _loggers.GetOrAdd(loggerName, () => LogManager.GetLogger(loggerName));
        }
    }

    internal class BotMessage : BaseJournalMessage
    {
        public BotMessage(DateTime time, string botName, string message, JournalMessageType type) : base(time)
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
    }
}
