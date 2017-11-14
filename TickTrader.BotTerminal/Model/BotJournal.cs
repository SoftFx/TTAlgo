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

        public BotJournal() : this(500) { }

        public BotNameAggregator Statistics { get; private set; }

        public BotJournal(int journalSize) : base(journalSize)
        {
            Statistics = new BotNameAggregator();
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

        public void LogStatus(string botName, string status)
        {
            GetOrAddLogger(botName).Trace(status);
        }

        protected override void OnAppended(BotMessage item)
        {
            Statistics.Register(item);
        }

        protected override void OnRemoved(BotMessage item)
        {
            Statistics.UnRegister(item);
        }

        public void RegisterBotLog(string botName)
        {
        }

        public void UnregisterBotLog(string botName)
        {
            _loggers.Remove(botName);
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
