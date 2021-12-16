using System;

namespace TickTrader.Algo.Account.Settings
{
    internal interface IMonitoringSetting
    {
        Action<string> NotificationMethod { get; set; }
    }


    internal interface IQuoteMonitoring : IMonitoringSetting
    {
        bool EnableQuoteMonitoring { get; set; }

        int AccetableQuoteDelay { get; set; }

        int AlertsDelay { get; set; }
    }


    public sealed class AccountMonitoringSettings : IQuoteMonitoring
    {
        public bool EnableQuoteMonitoring { get; set; }

        public int AccetableQuoteDelay { get; set; }

        public int AlertsDelay { get; set; }


        public Action<string> NotificationMethod { get; set; }
    }
}