using System;
using TickTrader.BotAgent.Configurator.Properties;

namespace TickTrader.BotAgent.Configurator
{
    public class ProtocolViewModel : BaseContentViewModel
    {
        private readonly RefreshCounter _refreshManager;
        private readonly string _keyPort, _keyDirectory, _keyLog;

        private ProtocolModel _model;

        public delegate void ChangeListeningPort();

        public event ChangeListeningPort ChangeListeningPortEvent;

        public ProtocolViewModel(ProtocolModel model, RefreshCounter refManager = null) : base(nameof(ProtocolViewModel))
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

                if (!int.TryParse(value, out int port))
                {
                    ErrorCounter.AddError(_keyPort);
                    _refreshManager?.AddUpdate(_keyPort);
                    throw new ArgumentException(Resources.FieldAsNumberEx);
                }

                _model.ListeningPort = port;
                _refreshManager?.CheckUpdate(value, _model.CurrentListeningPort.ToString(), _keyPort);

                ErrorCounter.DeleteError(_keyPort);
                ErrorCounter.DeleteWarning(_keyPort);
                OnPropertyChanged(nameof(ListeningPort));
                ChangeListeningPortEvent.Invoke();
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

                OnPropertyChanged(nameof(LogMessage));
            }
        }

        public Uri ListeningUri => _model.ListeningUri;

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
                            ErrorCounter.CheckNumberRange(_model.ListeningPort.Value, _keyPort, max: (1 << 16) - 1);
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
                    ErrorCounter.DeleteError(_keyPort);
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
