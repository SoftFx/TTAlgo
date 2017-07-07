using Newtonsoft.Json;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Models
{
    public class AppSettings
    {
        [JsonProperty("server.urls")]
        public string ServerUrls { get; set; }
    }
}
