using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol
{
    public class SubscribeReportEntity : ReportEntity
    {
    }


    internal static class SubscribeReportEntityExtensions
    {
        internal static SubscribeReportEntity ToEntity(this SubscribeReport report)
        {
            return new SubscribeReportEntity { RequestId = report.RequestId };
        }

        internal static SubscribeReport ToMessage(this SubscribeReportEntity report)
        {
            return new SubscribeReport(0) { RequestId = report.RequestId };
        }
    }
}
