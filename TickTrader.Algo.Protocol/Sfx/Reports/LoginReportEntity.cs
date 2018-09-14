using SoftFX.Net.BotAgent;
using System;

namespace TickTrader.Algo.Protocol.Sfx
{
    public class LoginReportEntity
    {
        [Obsolete]
        public int CurrentVersion { get; set; }


        public LoginReportEntity() { }
    }


    internal static class LoginReportEntityExtensions
    {
        internal static LoginReportEntity ToEntity(this LoginReport report)
        {
            return new LoginReportEntity();
        }

        internal static LoginReport ToMessage(this LoginReportEntity report)
        {
            return new LoginReport(0);
        }

        internal static LoginReportEntity ToEntity(this LoginReport_1 report)
        {
            return new LoginReportEntity { CurrentVersion = report.CurrentVersion };
        }

        internal static LoginReport_1 ToMessage_1(this LoginReportEntity report)
        {
            return new LoginReport_1(0) { CurrentVersion = report.CurrentVersion };
        }
    }
}
