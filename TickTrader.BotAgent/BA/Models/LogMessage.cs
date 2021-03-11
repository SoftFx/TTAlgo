using Google.Protobuf.WellKnownTypes;
using TickTrader.Algo.Domain;

namespace TickTrader.BotAgent.BA.Models
{
    public class LogEntry : ILogEntry
    {
        private readonly PluginLogRecord _record;

        public LogEntry(PluginLogRecord record)
        {
            _record = record;
        }

        public PluginLogRecord.Types.LogSeverity Severity => _record.Severity;

        public string Message => _record.Message;

        public Timestamp TimeUtc => _record.TimeUtc;

        public override string ToString()
        {
            return $"{Severity.ToString().ToUpper().PadLeft(7)} | {Message}";
        }
    }
}
