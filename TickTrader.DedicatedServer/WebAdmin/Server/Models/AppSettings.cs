using Newtonsoft.Json;
using TickTrader.DedicatedServer.WebAdmin.Server.Core.Crypto;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Models
{
    public class AppSettings
    {
        [JsonProperty("server.urls")]
        public string ServerUrls { get; set; }
        public string SecretKey { get; set; }
        public ServerCredentials Credentials { get; set; }


        public static AppSettings Default => new AppSettings { ServerUrls = DefaultServerUrl, Credentials = DefaultCredentials, SecretKey = RandomSecretKey };

        public static string DefaultServerUrl => @"http://localhost:5000/";
        public static string RandomSecretKey => new KeyGenerator().GetUniqueKey(128);
        public static ServerCredentials DefaultCredentials => new ServerCredentials("Administrator", "Administrator");
    }
}
