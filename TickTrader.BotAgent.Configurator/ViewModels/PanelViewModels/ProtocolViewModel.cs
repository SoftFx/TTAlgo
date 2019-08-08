using System;

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

        public string ListeningPort
        {
            get => _model.ListeningPort.ToString();

            set
            {
                if (_model.ListeningPort.ToString() == value)
                    return;

                try
                {
                    int listeningPort = int.Parse(value);
                    _model.ListeningPort = listeningPort;

                    ErrorCounter.CheckNumberRange(listeningPort, nameof(ListeningPort), max: 1 << 16);

                    _model.CheckPort(listeningPort);

                    ErrorCounter.DeleteError(nameof(ListeningPort));
                }
                catch (Exception ex)
                {
                    ErrorCounter.AddError(nameof(ListeningPort));
                    throw ex;
                }

                _logger.Info(GetChangeMessage($"{nameof(ProtocolViewModel)} {nameof(ListeningPort)}", _model.ListeningPort.ToString(), value.ToString()));

                _refreshManager?.Refresh();

                ErrorCounter.DeleteError(nameof(ListeningPort));
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

                _model.DirectoryName = value;

                ErrorCounter.CheckStringLength(DirectoryName, 1, nameof(DirectoryName));

                _refreshManager?.Refresh();
                _logger.Info(GetChangeMessage($"{nameof(ProtocolViewModel)} {nameof(DirectoryName)}", _model.DirectoryName, value));

                ErrorCounter.DeleteError(nameof(DirectoryName));
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
    }
}
