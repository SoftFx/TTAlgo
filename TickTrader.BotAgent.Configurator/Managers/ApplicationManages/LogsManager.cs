using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TickTrader.BotAgent.Configurator
{
    public class LogsManager
    {
        private const int MaxMessagesCount = 1000;

        private readonly string _logsFilePath = Path.Combine(Environment.CurrentDirectory, "Logs.log");

        private readonly string[] separators = new string[] { Environment.NewLine };

        private LinkedList<string> _messages;

        public LogsManager()
        {
            _messages = new LinkedList<string>();
        }

        public string LogsStr => string.Join(Environment.NewLine, _messages);

        public void UpdateLog()
        {
            try
            {
                string str = string.Empty;

                using (var sr = new StreamReader(_logsFilePath))
                {
                    str = sr.ReadToEnd();
                }

                var messagesList = str.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                messagesList.Reverse();

                if (messagesList.Count() > MaxMessagesCount)
                    messagesList = messagesList.Take(MaxMessagesCount).ToArray();

                _messages = new LinkedList<string>(messagesList.ToList());
            }
            catch
            { }
        }
    }
}
