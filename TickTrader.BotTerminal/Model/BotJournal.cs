using Caliburn.Micro;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TickTrader.BotTerminal
{
    internal class BotJournal : Journal<BotMessage>
    {
        public BotJournal() : this(500) { }
        public BotJournal(int journalSize) : base(journalSize)
        {
        }

        public void Info(string botName, string message)
        {
            Info(botName, message, new object[0]);
        }

        public void Info(string botName, string message, params object[] args)
        {
            Add(new BotMessage(botName, string.Format(message, args), JournalMessageType.Info));
        }

        public void Error(string botName, string message, params object[] args)
        {
            Add(new BotMessage(botName, string.Format(message, args), JournalMessageType.Error));
        }

        public void Trading(string botName, string message)
        {
            Trading(botName, message, new object[0]);
        }

        public void Trading(string botName, string message, params object[] args)
        {
            Add(new BotMessage(botName, string.Format(message, args), JournalMessageType.Trading));
        }
    }

    internal class BotMessage : BaseJournalMessage
    {
        public BotMessage(string botName, string message)
        {
            Message = message;
            Bot =  botName;
        }

        public BotMessage(string botName, string message, JournalMessageType type) : this(botName, message)
        {
            Type = type;
        }

        public string Bot { get; set; }
    }
}
