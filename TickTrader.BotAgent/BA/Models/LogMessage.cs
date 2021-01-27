using System;
using TickTrader.Algo.Core;

namespace TickTrader.BotAgent.BA.Models
{
    public class LogEntry : ILogEntry
    {
        public LogEntry(TimeKey key, LogEntryType type, string message)
        {
            TimeUtc = new TimeKey(DateTime.UtcNow, 0);
            Type = type;
            Message = message;
        }

        public LogEntryType Type { get; private set; }

        public string Message { get; private set; }

        public TimeKey TimeUtc { get; private set; }

        public override string ToString()
        {
            return $"{Type.ToString().ToUpper().PadLeft(7)} | {Message}";
        }
    }
}
