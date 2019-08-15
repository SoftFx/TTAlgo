namespace TickTrader.BotAgent.Configurator
{
    public class ProtocolModel : IWorkingModel
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly PortsManager _portManager;

        private const string DefaultDirectoryName = "Logs";
        private const int DefaultPort = 8443;

        public ProtocolModel(PortsManager manager)
        {
            _portManager = manager;
        }

        public int? ListeningPort { get; set; }

        public int CurrentListeningPort { get; set; }

        public string DirectoryName { get; set; }

        public string CurrentDirectoryName { get; set; }

        public bool LogMessage { get; set; }

        public bool CurrentLogMessage { get; set; }

        public void SetDefaultValues()
        {
            DirectoryName = DirectoryName ?? DefaultDirectoryName;

            if (!ListeningPort.HasValue)
                ListeningPort = DefaultPort;
        }

        public void UpdateCurrentFields()
        {
            CurrentDirectoryName = DirectoryName;
            CurrentListeningPort = ListeningPort.Value;
            CurrentLogMessage = LogMessage;
        }

        public void CheckPort(int port)
        {
            _portManager.CheckPort(port);
        }
    }
}
