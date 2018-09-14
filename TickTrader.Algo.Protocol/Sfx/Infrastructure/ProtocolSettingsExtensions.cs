using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol.Sfx
{
    internal static class ProtocolSettingsExtensions
    {
        internal static ServerOptions CreateServerOptions(this IServerSettings settings)
        {
            var serverOptions = new ServerOptions(settings.ProtocolSettings.ListeningPort)
            {
                ConnectionType = SoftFX.Net.Core.ConnectionType.Secure,
                Certificate = settings.Certificate,
                RequireClientCertificate = false,
                SessionThreadCount = 3,
            };

            serverOptions.Log.Directory = settings.ProtocolSettings.LogDirectoryName;
            serverOptions.Log.Events = settings.ProtocolSettings.LogEvents;
            serverOptions.Log.States = settings.ProtocolSettings.LogStates;
            serverOptions.Log.Messages = settings.ProtocolSettings.LogMessages;

            return serverOptions;
        }

        internal static ClientSessionOptions CreateClientOptions(this IClientSessionSettings settings)
        {
            var clientSessionOptions = new ClientSessionOptions(settings.ProtocolSettings.ListeningPort)
            {
                ServerCertificateName = settings.ServerCertificateName,
                ConnectionType = SoftFX.Net.Core.ConnectionType.Secure,
                ConnectMaxCount = 1,
                ReconnectMaxCount = 0,
            };

            clientSessionOptions.Log.Directory = settings.ProtocolSettings.LogDirectoryName;
            clientSessionOptions.Log.Events = settings.ProtocolSettings.LogEvents;
            clientSessionOptions.Log.States = settings.ProtocolSettings.LogStates;
            clientSessionOptions.Log.Messages = settings.ProtocolSettings.LogMessages;

            return clientSessionOptions;
        }
    }
}
