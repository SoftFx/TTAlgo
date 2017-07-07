using Microsoft.Extensions.Configuration;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Extensions
{
    public static class ConfigurationExtensions
    {
        public static string GetSecretKey(this IConfiguration configuration)
        {
            return configuration.GetValue<string>("SecretKey");
        }
    }
}
