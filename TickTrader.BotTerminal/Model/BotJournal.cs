using Machinarium.Qnil;
using NLog;
using System.Collections.Generic;
using System;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    internal class BotJournal
    {
        private Dictionary<string, Logger> _loggers = new Dictionary<string, Logger>();

        private int _journalSize;

        public Dictionary<string, Journal<BotMessage>> Records = new Dictionary<string, Journal<BotMessage>>();

        public BotJournal() : this(500) { }

        public BotNameAggregator Statistics { get; private set; }

        public BotJournal(int journalSize)
        {
            _journalSize = journalSize;
            Statistics = new BotNameAggregator();
        }

        public void Add(BotMessage item)
        {
            if (!Records.ContainsKey(item.Bot))
                Records.Add(item.Bot, new Journal<BotMessage>(_journalSize));

            Records[item.Bot].Add(item);

            WriteToLogger(item);
        }

        public void Add(List<BotMessage> items)
        {
            foreach (var item in items)
                Add(item);

            foreach (var item in items)
                WriteToLogger(item);
        }

        public void LogStatus(string botName, string status)
        {
            GetOrAddLogger(botName).Trace(status);
        }

        protected void OnAppended(BotMessage item)
        {
            Statistics.Register(item);
        }

        protected void OnRemoved(BotMessage item)
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
