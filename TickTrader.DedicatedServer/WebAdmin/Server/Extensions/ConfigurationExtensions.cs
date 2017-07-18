using Microsoft.Extensions.Configuration;
using TickTrader.DedicatedServer.WebAdmin.Server.Models;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Extensions
{
    public static class ConfigurationExtensions
    {
        public static string GetSecretKey(this IConfiguration configuration)
        {
            return configuration.GetValue<string>(nameof(AppSettings.SecretKey));
        }

        public static ServerCredentials GetCredentials(this IConfiguration configuration)
        {
            return configuration.GetSection(nameof(AppSettings.Credentials)).Get<ServerCredentials>();
        }

        public static SslSettings GetSslSettings(this IConfiguration configuration)
        {
            return configuration.GetSection(nameof(AppSettings.Ssl)).Get<SslSettings>();
        }
    }
}
