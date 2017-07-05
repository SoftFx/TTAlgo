using Newtonsoft.Json;

namespace TickTrader.DedicatedServer.WebAdmin
{
    public class AppSettings
    {
        [JsonProperty("server.urls")]
        public string ServerUrls { get; set; }
    }
}
