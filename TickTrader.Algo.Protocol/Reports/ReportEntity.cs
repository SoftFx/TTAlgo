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

        internal ReportEntity(Report report)
        {
            RequestId = report.RequestId;
        }
    }


    internal static class ReportEntityExtensions
    {
        internal static ReportEntity ToEntity(this Report report)
        {
            var res = new ReportEntity { RequestId = report.RequestId };
            return res;
        }

        internal static Report ToMessage(this ReportEntity report)
        {
            var res = new Report(0) { RequestId = report.RequestId };
            return res;
        }
    }
}
