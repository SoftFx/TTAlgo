using System;

namespace TickTrader.Algo.Account.Settings
{
    public sealed class AccountModelSettings
    {
        public string LoggerId { get; }


        // change all properties to init
        public ConnectionSettings ConnectionSettings { get; set; }

        //public HistoryProviderSettings HistoryProviderSettings { get; set; }

        public AccountMonitoringSettings Monitoring { get; set; }


        public AccountModelSettings(string loggerId)
        {
            LoggerId = loggerId;
        }
    }


    public sealed class ConnectionSettings
    {
        public ServerInteropFactory AccountFactory { get; set; }

        public ConnectionOptions Options { get; set; }
    }
}