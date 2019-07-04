namespace TickTrader.BotAgent.Configurator
{
    public class ProtocolViewModel : BaseViewModel, IContentViewModel
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private ProtocolModel _model;
        private RefreshManager _refreshManager;

        public ProtocolViewModel(ProtocolModel model, RefreshManager refManager = null)
        {
            _model = model;
            _refreshManager = refManager;
        }

        public int ListeningPort
        {
            get => _model.ListeningPort;

            set
            {
                if (_model.ListeningPort == value)
                    return;

                _logger.Info(GetChangeMessage($"{nameof(ProtocolViewModel)} {nameof(ListeningPort)}", _model.ListeningPort.ToString(), value.ToString()));

                _model.ListeningPort = value;
                _refreshManager?.Refresh();

                OnPropertyChanged(nameof(ListeningPort));
            }
        }

        public string DirectoryName
        {
            get => _model.DirectoryName;

            set
            {
                if (_model.DirectoryName == value)
                    return;

                _logger.Info(GetChangeMessage($"{nameof(ProtocolViewModel)} {nameof(DirectoryName)}", _model.DirectoryName, value));

                _model.DirectoryName = value;
                _refreshManager?.Refresh();

                OnPropertyChanged(nameof(DirectoryName));
            }
        }

        public bool LogMessage
        {
            get => _model.LogMessage;

            set
            {
                if (_model.LogMessage == value)
                    return;

                _logger.Info(GetChangeMessage($"{nameof(ProtocolViewModel)} {nameof(LogMessage)}", _model.LogMessage.ToString(), value.ToString()));

                _model.LogMessage = value;
                _refreshManager?.Refresh();

                OnPropertyChanged(nameof(LogMessage));
            }
        }

        public string ModelDescription { get; set; }

        public string ListeningPortDescription { get; set; }

        public string DirectoryNameDescription { get; set; }

        public string LogMessageDescription { get; set; }

        public void RefreshModel()
        {
            OnPropertyChanged(nameof(ListeningPort));
            OnPropertyChanged(nameof(DirectoryName));
            OnPropertyChanged(nameof(LogMessage));
        }

        public void CheckPort(int port = -1)
        {
            _model.CheckPort(port == -1 ? ListeningPort : port);
        }
    }
}
