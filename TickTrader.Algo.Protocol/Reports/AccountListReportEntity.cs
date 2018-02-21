using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol
{
    public class AccountListReportEntity : ReportEntity
    {
        public AccountModelEntity[] Accounts { get; set; }


        public AccountListReportEntity() : base()
        {
            Accounts = new AccountModelEntity[0];
        }
    }


    internal static class AccountListReportEntityExtensions
    {
        internal static AccountListReportEntity ToEntity(this AccountListReport_1 report)
        {
            var res = new AccountListReportEntity { RequestId = report.RequestId, RequestState = ToAlgo.Convert(report.RequestState), Text = report.Text };
            res.Accounts = new AccountModelEntity[report.Accounts.Length];
            for (var i = 0; i < report.Accounts.Length; i++)
            {
                res.Accounts[i] = new AccountModelEntity();
                res.Accounts[i].UpdateSelf(report.Accounts[i]);
            }
            return res;
        }

        internal static AccountListReport_1 ToMessage_1(this AccountListReportEntity report)
        {
            var res = new AccountListReport_1(0) { RequestId = report.RequestId, RequestState = ToSfx.Convert(report.RequestState), Text = report.Text };
            res.Accounts.Resize(report.Accounts.Length);
            for (var i = 0; i < report.Accounts.Length; i++)
            {
                report.Accounts[i].UpdateModel(res.Accounts[i]);
            }
            return res;
        }

        internal static AccountListReportEntity ToEntity(this AccountListReport report)
        {
            var res = new AccountListReportEntity { RequestId = report.RequestId, RequestState = ToAlgo.Convert(report.RequestState), Text = report.Text };
            res.Accounts = new AccountModelEntity[report.Accounts.Length];
            for (var i = 0; i < report.Accounts.Length; i++)
            {
                res.Accounts[i] = new AccountModelEntity();
                res.Accounts[i].UpdateSelf(report.Accounts[i]);
            }
            return res;
        }

        internal static AccountListReport ToMessage(this AccountListReportEntity report)
        {
            var res = new AccountListReport(0) { RequestId = report.RequestId, RequestState = ToSfx.Convert(report.RequestState), Text = report.Text };
            res.Accounts.Resize(report.Accounts.Length);
            for (var i = 0; i < report.Accounts.Length; i++)
            {
                report.Accounts[i].UpdateModel(res.Accounts[i]);
            }
            return res;
        }
    }
}
