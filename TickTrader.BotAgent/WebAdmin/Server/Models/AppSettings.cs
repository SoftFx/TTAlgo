﻿using Newtonsoft.Json;
using TickTrader.BotAgent.Common;

namespace TickTrader.BotAgent.WebAdmin.Server.Models
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

        public static AppSettings Default => new AppSettings
        {
            ServerUrls = DefaultServerUrl,
            Credentials = DefaultCredentials,
            SecretKey = RandomSecretKey,
            Ssl = DefaultSslSettings,
            Protocol = DefaultProtocolSettings,
            Fdk = DefaultFdkSettings,
        };

        public static string DefaultServerUrl => @"https://localhost:5001/;http://localhost:5000";
        public static string RandomSecretKey => new KeyGenerator().GetUniqueKey(128);
        public static ServerCredentials DefaultCredentials => new ServerCredentials("Administrator", "Administrator", "Dealer", "Dealer", "Viewer", "Viewer");
        public static SslSettings DefaultSslSettings => new SslSettings { File = "certificate.pfx", Password = "" };
        public static ProtocolSettings DefaultProtocolSettings => new ProtocolSettings
        {
            ListeningPort = 8443,
            LogDirectoryName = "Logs",
            LogMessages = false,
        };
        public static FdkSettings DefaultFdkSettings => new FdkSettings
        {
            EnableLogs = false,
        };
    }
}