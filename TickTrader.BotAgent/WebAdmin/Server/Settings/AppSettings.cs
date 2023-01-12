using Newtonsoft.Json;
using System.IO;
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

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public NotificationSettings Notification { get; set; }


        public static string DefaultServerUrl => @"https://localhost:15139/;http://localhost:15137";

        public static string RandomSecretKey => new KeyGenerator().GetUniqueKey(128);

        public static ServerCredentials DefaultCredentials => new("Administrator", "Administrator", "Dealer", "Dealer", "Viewer", "Viewer");

        public static SslSettings DefaultSslSettings => new()
        {
            File = "certificate.pfx",
            Password = ""
        };

        public static ProtocolSettings DefaultProtocolSettings => new()
        {
            ListeningPort = 15443,
            LogDirectoryName = "Logs",
            LogMessages = false,
        };

        public static FdkSettings DefaultFdkSettings => new()
        {
            EnableLogs = false,
        };

        public static AlgoSettings DefaultAlgoSettigns => new()
        {
            EnableDevMode = false,
        };

        public static MonitoringSettings DefaultMonitoringSettings => new()
        {
            QuoteMonitoring = QuoteMonitoringSettings.Default,
        };

        public static NotificationSettings DefaultNotoficationSettings => new()
        {
            Telegram = TelegramSettings.Default,
        };


        public static AppSettings Default => new()
        {
            ServerUrls = DefaultServerUrl,
            Credentials = DefaultCredentials,
            SecretKey = RandomSecretKey,
            Ssl = DefaultSslSettings,
            Protocol = DefaultProtocolSettings,
            Fdk = DefaultFdkSettings,
            Algo = DefaultAlgoSettigns,
            Monitoring = DefaultMonitoringSettings,
            Notification = DefaultNotoficationSettings,
        };


        public static void EnsureValidConfiguration(string filePath)
        {
            if (!File.Exists(filePath))
            {
                SaveSettings(filePath, Default);
            }
            else
            {
                MigrateSettings(filePath);
            }
        }


        private static void SaveSettings(string filePath, AppSettings appSettings)
        {
            File.WriteAllText(filePath, JsonConvert.SerializeObject(appSettings, Formatting.Indented));
        }

        private static void MigrateSettings(string filePath)
        {
            var currentSettings = JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(filePath));

            var anyChanges = false;

            if (currentSettings.Protocol == null)
            {
                currentSettings.Protocol = Default.Protocol;
                anyChanges = true;
            }

            if (currentSettings.Credentials.Login != null)
            {
                var oldCreds = currentSettings.Credentials;
                currentSettings.Credentials = Default.Credentials;
                currentSettings.Credentials.AdminLogin = oldCreds.Login;
                currentSettings.Credentials.AdminPassword = oldCreds.Password;
                anyChanges = true;
            }

            if (currentSettings.Fdk == null)
            {
                currentSettings.Fdk = Default.Fdk;
                anyChanges = true;
            }

            if (currentSettings.Algo == null)
            {
                currentSettings.Algo = Default.Algo;
                anyChanges = true;
            }

            if (currentSettings.Monitoring == null)
            {
                currentSettings.Monitoring = Default.Monitoring;
                anyChanges = true;
            }

            if (currentSettings.Notification == null)
            {
                currentSettings.Notification = Default.Notification;
                anyChanges = true;
            }

            if (anyChanges)
            {
                SaveSettings(filePath, currentSettings);
            }
        }
    }
}
