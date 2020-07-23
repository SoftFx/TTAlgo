using Google.Protobuf.WellKnownTypes;

namespace TickTrader.Algo.Common.Info
{
    public class LogRecordInfo
    {
        public Timestamp TimeUtc { get; set; }

        public Domain.UnitLogRecord.Types.LogSeverity Severity { get; set; }

        public string Message { get; set; }


        public LogRecordInfo()
        {
        }
    }

    public class AlertRecordInfo
    {
        public Timestamp TimeUtc { get; set; }

        public string Message { get; set; }

        public string BotId { get; set; }

        public AlertRecordInfo()
        {
        }
    }
}
