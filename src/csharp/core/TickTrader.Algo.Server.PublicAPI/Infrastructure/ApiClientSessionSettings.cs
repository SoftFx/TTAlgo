using TickTrader.Algo.Server.Common;

namespace TickTrader.Algo.Server.PublicAPI
{
    internal sealed class ApiClientSessionSettings : ClientSessionSettings, IClientSessionSettings
    {
        public ApiClientSessionSettings(string serverAddress, int serverPort, string login, string password, string logDirectory, bool logMessages)
            :base(serverAddress, serverPort, login, password, logDirectory, logMessages) { }
    }
}
