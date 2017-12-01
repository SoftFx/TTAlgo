using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using TickTrader.BotAgent.Extensions;
using TickTrader.BotAgent.WebAdmin.Server.Models;

namespace TickTrader.BotAgent.WebAdmin.Server.Extensions
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

        public static ProtocolSettings GetProtocolSettings(this IConfiguration configuration)
        {
            return configuration.GetSection(nameof(AppSettings.Protocol)).Get<ProtocolSettings>();
        }

        public static X509Certificate2 GetCertificate(this IConfiguration config, string contentRoot)
        {
            var sslConf = config.GetSslSettings();

            if (sslConf == null)
                throw new ArgumentException("SSL configuration not found");

            if (string.IsNullOrWhiteSpace(sslConf.File))
                throw new ArgumentException("Certificate file is not defined");

            var pfxFile = sslConf.File;

            if (!pfxFile.IsPathAbsolute())
                pfxFile = Path.Combine(contentRoot, pfxFile);

            return new X509Certificate2(pfxFile, sslConf.Password);
        }

        public static ProtocolServerSettings GetProtocolServerSettings(this IConfiguration config, string contentRoot)
        {
            var creds = config.GetCredentials();

            if (creds == null)
                throw new ArgumentException("Server credentials not found");

            var protocolConfig = config.GetProtocolSettings();

            if (protocolConfig == null)
                throw new ArgumentException("Protocol configuration not found");

            if (protocolConfig.ListeningPort < 0 || protocolConfig.ListeningPort > 65535)
                throw new ArgumentException("Invalid port number");

            var certificate = config.GetCertificate(contentRoot);

            var serverSettings = new ProtocolServerSettings
            {
                ServerName = "BotAgentServer",
                Certificate = certificate,
                ProtocolSettings = protocolConfig,
                Login = creds.Login,
                Password = creds.Password,
            };

            return serverSettings;
        }
    }
}
