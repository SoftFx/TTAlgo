using System;

namespace TickTrader.BotAgent.Configurator
{
    public class ProtocolViewModel : BaseViewModel
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly RefreshManager _refreshManager;
        private readonly string _keyPort, _keyDirectory, _keyLog;

        private ProtocolModel _model;

        public ProtocolViewModel(ProtocolModel model, RefreshManager refManager = null) : base(nameof(ProtocolViewModel))
        {
            _model = model;
            _refreshManager = refManager;

            _keyPort = $"{nameof(ProtocolViewModel)} {nameof(ListeningPort)}";
            _keyDirectory = $"{nameof(ProtocolViewModel)} {nameof(DirectoryName)}";
            _keyLog = $"{nameof(ProtocolViewModel)} {nameof(_keyPort)}";
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
                    _model.ListeningPort = int.Parse(value);
                }
                catch (Exception ex)
                {
                    ErrorCounter.AddError(_keyPort);
                    _refreshManager?.AddUpdate(_keyPort);
                    _model.ListeningPort = 0;
                    throw ex;
                }

                _refreshManager?.CheckUpdate(value, _model.CurrentListeningPort.ToString(), _keyPort);
                _logger.Info(GetChangeMessage(_keyPort, _model.ListeningPort.ToString(), value));

                ErrorCounter.DeleteError(_keyPort);
                ErrorCounter.DeleteWarning(_keyPort);
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

                _refreshManager?.CheckUpdate(value, _model.CurrentDirectoryName, _keyDirectory);
                _logger.Info(GetChangeMessage(_keyDirectory, _model.DirectoryName, value));

                ErrorCounter.DeleteError(_keyDirectory);
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

                _model.LogMessage = value;

                _refreshManager?.CheckUpdate(value.ToString(), _model.CurrentLogMessage.ToString(), _keyLog);
                _logger.Info(GetChangeMessage(_keyLog, _model.LogMessage.ToString(), value.ToString()));

                OnPropertyChanged(nameof(LogMessage));
            }
        }

        public override string this[string columnName]
        {
            get
            {
                var msg = string.Empty;

                try
                {
                    switch (columnName)
                    {
                        case "ListeningPort":
                            ErrorCounter.CheckNumberRange(_model.ListeningPort.Value, _keyPort, max: 1 << 16);
                            _model.CheckPort(_model.ListeningPort.Value);
                            break;
                        case "DirectoryName":
                            ErrorCounter.CheckStringLength(DirectoryName, 1, _keyDirectory);
                            break;
                    }
                }
                catch (WarningException ex)
                {
                    msg = ex.Message;
                    ErrorCounter.AddWarning(_keyPort);
                }
                catch (Exception ex)
                {
                    msg = ex.Message;
                }

                return msg;
            }
        }

        public string ListeningPortDescription { get; set; }

        public string DirectoryNameDescription { get; set; }

        public string LogMessageDescription { get; set; }

        public override void RefreshModel()
        {
            OnPropertyChanged(nameof(ListeningPort));
            OnPropertyChanged(nameof(DirectoryName));
            OnPropertyChanged(nameof(LogMessage));
        }
    }
}
