using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotAgent.Configurator
{
    public class ProtocolViewModel : INotifyPropertyChanged
    {
        private ProtocolModel _model;

        public ProtocolViewModel(ProtocolModel model)
        {
            _model = model;
        }

        public int ListeningPort
        {
            get
            {
                return _model.ListeningPort;
            }

            set
            {
                if (_model.ListeningPort == value)
                    return;

                _model.ListeningPort = value;
                OnPropertyChanged(nameof(ListeningPort));
            }
        }

        public string DirectoryName
        {
            get
            {
                return _model.DirectoryName;
            }

            set
            {
                if (_model.DirectoryName == value)
                    return;

                _model.DirectoryName = value;
                OnPropertyChanged(nameof(DirectoryName));
            }
        }

        public bool LogMessage
        {
            get
            {
                return _model.LogMessage;
            }

            set
            {
                if (_model.LogMessage == value)
                    return;

                _model.LogMessage = value;
                OnPropertyChanged(nameof(LogMessage));
            }
        }

        public void RefreshModel()
        {
            OnPropertyChanged(nameof(ListeningPort));
            OnPropertyChanged(nameof(DirectoryName));
            OnPropertyChanged(nameof(LogMessage));
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
