using System;

namespace TickTrader.DedicatedServer.DS.Models
{
    public class LogMessage : ILogMessage
    {
        public LogMessage(LogMessageType type, string message)
        {
            TimeUtc = DateTime.UtcNow;
            Type = type;
            Message = message;
        }

        public LogMessageType Type { get; private set; }

        public string Message { get; private set; }

        public DateTime TimeUtc { get; private set; }

        public override string ToString()
        {
            return $"{Type.ToString().ToUpper().PadLeft(7)} | {Message}";
        }
    }
}
