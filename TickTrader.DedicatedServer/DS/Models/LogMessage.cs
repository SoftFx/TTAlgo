using System;

namespace TickTrader.DedicatedServer.DS.Models
{
    public class LogEntry : ILogEntry
    {
        public LogEntry(LogEntryType type, string message)
        {
            TimeUtc = DateTime.UtcNow;
            Type = type;
            Message = message;
        }

        public LogEntryType Type { get; private set; }

        public string Message { get; private set; }

        public DateTime TimeUtc { get; private set; }

        public override string ToString()
        {
            return $"{Type.ToString().ToUpper().PadLeft(7)} | {Message}";
        }
    }
}
