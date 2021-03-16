namespace TickTrader.Algo.ServerControl
{
    public class ClientSessionSettings
    {
        public string ServerAddress { get; }

        public int ServerPort { get; }

        public string Login { get; }

        public string Password { get; }

        public string LogDirectory { get; }

        public bool LogMessages { get; }


        public ClientSessionSettings(string serverAddress, int serverPort, string login, string password)
            : this(serverAddress, serverPort, login, password, "Logs", false)
        { }

        public ClientSessionSettings(string serverAddress, int serverPort, string login, string password, string logDirectory, bool logMessages)
        {
            ServerAddress = serverAddress;
            ServerPort = serverPort;
            Login = login;
            Password = password;
            LogDirectory = logDirectory;
            LogMessages = logMessages;
        }
    }
}
