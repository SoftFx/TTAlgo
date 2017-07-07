using Newtonsoft.Json;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Models
{
    public class ServerCredentials
    {
        public ServerCredentials()
        {

        }

        public ServerCredentials(string loign, string password)
        {
            Login = loign;
            Password = password;
        }

        [JsonProperty("login")]
        public string Login { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
