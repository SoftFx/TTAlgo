using Newtonsoft.Json;
using TickTrader.BotAgent.WebAdmin.Server.Core.Crypto;

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

        public static AppSettings Default => new AppSettings
        {
            ServerUrls = DefaultServerUrl,
            Credentials = DefaultCredentials,
            SecretKey = RandomSecretKey,
            Ssl = new SslSettings { File = "certificate.pfx", Password = "" }
        };

        public static string DefaultServerUrl => @"https://localhost:5000/";
        public static string RandomSecretKey => new KeyGenerator().GetUniqueKey(128);
        public static ServerCredentials DefaultCredentials => new ServerCredentials("Administrator", "Administrator");
    }
}
