using SoftFX.Net.BotAgent;
using System;

namespace TickTrader.Algo.Protocol
{
    public class ReportEntity
    {
        public string RequestId { get; set; }


        public ReportEntity()
        {
            RequestId = Guid.NewGuid().ToString();
        }
    }


    internal static class ReportEntityExtensions
    {
        internal static ReportEntity ToEntity(this Report report)
        {
            return new ReportEntity { RequestId = report.RequestId };
        }

        internal static Report ToMessage(this ReportEntity report)
        {
            return new Report(0) { RequestId = report.RequestId };
        }
    }
}
