using Google.Protobuf.WellKnownTypes;

namespace TickTrader.Algo.Domain
{
    public partial class PluginLogRecord
    {
        public PluginLogRecord(Timestamp timeUtc, Types.LogSeverity severity, string msg, string details)
        {
            TimeUtc = timeUtc;
            Severity = severity;
            Message = msg;
            Details = details;
        }
    }
}
