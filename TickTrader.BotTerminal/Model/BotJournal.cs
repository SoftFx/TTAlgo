using Machinarium.Qnil;
using NLog;
using System.Collections.Generic;
using System;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    internal class BotJournal : Journal<BotMessage>
    {
        private Logger _logger;

        public BotMessageTypeCounter MessageCount { get; }

        public BotJournal(string botId) : this(botId, 1000) { }

        public BotJournal(string botId, int journalSize)
            : base(1000)
        {
            _logger = LogManager.GetLogger(LoggerHelper.GetBotLoggerName(botId));
            MessageCount = new BotMessageTypeCounter();
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

        public void LogStatus(string status)
        {
            _logger.Trace(status);
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

        private void WriteToLogger(BotMessage message)
        {
            if (message.Type != JournalMessageType.Error)
                _logger.Info(message.ToString());
            else
            {
                _logger.Error(message.ToString());
                if (message.Details != null)
                    _logger.Error(message.Details);
            }
        }

        private void LogError(BotMessage message)
        {
            _logger.Error(message.ToString());
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

    internal class BotMessageTypeCounter
    {
        private Dictionary<JournalMessageType, int> _messagesCnt;

        public int this[JournalMessageType type] => _messagesCnt[type];

        public BotMessageTypeCounter()
        {
            _messagesCnt = new Dictionary<JournalMessageType, int>();
            foreach (JournalMessageType type in Enum.GetValues(typeof(JournalMessageType)))
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
            foreach (JournalMessageType type in Enum.GetValues(typeof(JournalMessageType)))
            {
                _messagesCnt[type] = 0;
            }
        }
    }
}
