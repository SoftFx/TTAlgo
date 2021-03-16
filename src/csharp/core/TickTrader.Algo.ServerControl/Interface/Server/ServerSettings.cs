namespace TickTrader.Algo.ServerControl
{
    public class ServerSettings
    {
        public string ServerName { get; }

        public int ServerPort { get; }

        public string LogDirectory { get; }

        public bool LogMessages { get; }


        public ServerSettings(string serverName, int serverPort, string logDirectory, bool logMessages)
        {
            ServerName = serverName;
            ServerPort = serverPort;
            LogDirectory = logDirectory;
            LogMessages = logMessages;
        }
    }
}
