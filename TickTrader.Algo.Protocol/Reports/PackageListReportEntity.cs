using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol
{
    public class PackageListReportEntity : ReportEntity
    {
        public PackageModelEntity[] Packages { get; set; }


        public PackageListReportEntity() : base()
        {
            Packages = new PackageModelEntity[0];
        }
    }


    internal static class PackageListReportEntityExtensions
    {
        internal static PackageListReportEntity ToEntity(this PackageListReport report)
        {
            var res = new PackageListReportEntity { RequestId = report.RequestId };
            res.Packages = new PackageModelEntity[report.Packages.Length];
            for (var i = 0; i < report.Packages.Length; i++)
            {
                res.Packages[i] = new PackageModelEntity();
                res.Packages[i].UpdateSelf(report.Packages[i]);
            }
            return res;
        }

        internal static PackageListReport ToMessage(this PackageListReportEntity report)
        {
            var res = new PackageListReport(0) { RequestId = report.RequestId };
            res.Packages.Resize(report.Packages.Length);
            for (var i = 0; i < report.Packages.Length; i++)
            {
                report.Packages[i].UpdateModel(res.Packages[i]);
            }
            return res;
        }
    }
}
