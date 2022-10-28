using System;

namespace TickTrader.Algo.Account.Settings
{
    internal interface IMonitoringSetting
    {
        Action<string, string> NotificationMethod { get; }
    }


    internal interface IQuoteMonitoring : IMonitoringSetting
    {
        bool EnableQuoteMonitoring { get; }

        int AccetableQuoteDelay { get; }

        int AlertsDelay { get; }
    }


    public sealed record AccountMonitoringSettings : IQuoteMonitoring
    {
        public bool EnableQuoteMonitoring { get; init; }

        public int AccetableQuoteDelay { get; init; }

        public int AlertsDelay { get; init; }

        public Action<string, string> NotificationMethod { get; init; }
    }
}