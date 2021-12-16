using Newtonsoft.Json;
using TickTrader.Algo.Core.Lib;
using TickTrader.BotAgent.WebAdmin.Server.Models;

namespace TickTrader.BotAgent.WebAdmin.Server.Settings
{
    public class AppSettings
    {
        [JsonProperty("server.urls")]
        public string ServerUrls { get; set; }

        public string SecretKey { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ServerCredentials Credentials { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public SslSettings Ssl { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ProtocolSettings Protocol { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public FdkSettings Fdk { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public AlgoSettings Algo { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public MonitoringSettings Monitoring { get; set; }


        public static string DefaultServerUrl => @"https://localhost:15139/;http://localhost:15137";

        public static string RandomSecretKey => new KeyGenerator().GetUniqueKey(128);

        public static ServerCredentials DefaultCredentials => new ServerCredentials("Administrator", "Administrator", "Dealer", "Dealer", "Viewer", "Viewer");

        public static SslSettings DefaultSslSettings => new SslSettings
        {
            File = "certificate.pfx",
            Password = ""
        };

        public static ProtocolSettings DefaultProtocolSettings => new ProtocolSettings
        {
            ListeningPort = 15443,
            LogDirectoryName = "Logs",
            LogMessages = false,
        };

        public static FdkSettings DefaultFdkSettings => new FdkSettings
        {
            EnableLogs = false,
        };

        public static AlgoSettings DefaultAlgoSettigns => new AlgoSettings
        {
            EnableDevMode = false,
        };

        public static MonitoringSettings DefaultMonitoringSettings => new MonitoringSettings
        {
            QuoteMonitoring = QuoteMonitoringSettings.Default,
        };

        public static AppSettings Default => new AppSettings
        {
            ServerUrls = DefaultServerUrl,
            Credentials = DefaultCredentials,
            SecretKey = RandomSecretKey,
            Ssl = DefaultSslSettings,
            Protocol = DefaultProtocolSettings,
            Fdk = DefaultFdkSettings,
            Algo = DefaultAlgoSettigns,
            Monitoring = DefaultMonitoringSettings,
        };
    }
}
