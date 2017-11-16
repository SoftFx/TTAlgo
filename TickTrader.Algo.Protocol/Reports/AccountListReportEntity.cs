using SoftFX.Net.BotAgent;
using System;

namespace TickTrader.Algo.Protocol
{
    public class AccountListReportEntity
    {
        public string RequestId { get; internal set; }

        public AccountModelEntity[] Accounts { get; internal set; }


        public AccountListReportEntity()
        {
            RequestId = Guid.NewGuid().ToString();
            Accounts = new AccountModelEntity[0];
        }

        internal AccountListReportEntity(AccountListReport report)
        {
            RequestId = report.RequestId;
            Accounts = new AccountModelEntity[report.Accounts.Length];
            for (var i = 0; i < report.Accounts.Length; i++)
            {
                Accounts[i] = new AccountModelEntity();
                Accounts[i].UpdateSelf(report.Accounts[i]);
            }
        }
    }


    internal static class AccountListReportEntityExtensions
    {
        internal static AccountListReportEntity ToEntity(this AccountListReport report)
        {
            var res = new AccountListReportEntity { RequestId = report.RequestId };
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
            var res = new AccountListReport(0) { RequestId = report.RequestId };
            res.Accounts.Resize(report.Accounts.Length);
            for (var i = 0; i < report.Accounts.Length; i++)
            {
                report.Accounts[i].UpdateModel(res.Accounts[i]);
            }
            return res;
        }
    }
}
