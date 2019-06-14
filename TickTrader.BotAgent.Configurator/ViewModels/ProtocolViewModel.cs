using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TickTrader.BotAgent.Configurator
{
    public class ProtocolViewModel : IViewModel
    {
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

                _model.LogMessage = value;
                _refreshManager?.Refresh();

                OnPropertyChanged(nameof(LogMessage));
            }
        }

        public void RefreshModel()
        {
            OnPropertyChanged(nameof(ListeningPort));
            OnPropertyChanged(nameof(DirectoryName));
            OnPropertyChanged(nameof(LogMessage));
        }

        public void CheckPort()
        {
            _model.CheckListeningPort();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
