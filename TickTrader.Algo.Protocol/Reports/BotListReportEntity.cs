using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol
{
    public class BotListReportEntity : ReportEntity
    {
        public BotModelEntity[] Bots { get; set; }


        public BotListReportEntity() : base()
        {
            Bots = new BotModelEntity[0];
        }
    }


    internal static class BotListReportEntityExtensions
    {
        internal static BotListReportEntity ToEntity(this BotListReport report)
        {
            var res = new BotListReportEntity { RequestId = report.RequestId, RequestState = ToAlgo.Convert(report.RequestState), Text = report.Text };
            res.Bots = new BotModelEntity[report.Bots.Length];
            for (var i = 0; i < report.Bots.Length; i++)
            {
                res.Bots[i] = new BotModelEntity();
                res.Bots[i].UpdateSelf(report.Bots[i]);
            }
            return res;
        }

        internal static BotListReport ToMessage(this BotListReportEntity report)
        {
            var res = new BotListReport(0) { RequestId = report.RequestId, RequestState = ToSfx.Convert(report.RequestState), Text = report.Text };
            res.Bots.Resize(report.Bots.Length);
            for (var i = 0; i < report.Bots.Length; i++)
            {
                report.Bots[i].UpdateModel(res.Bots[i]);
            }
            return res;
        }
    }
}
