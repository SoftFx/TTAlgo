using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotAgent.Configurator
{
    public class ServerViewModel : INotifyPropertyChanged
    {
        private ServerModel _model;

        public ServerViewModel(ServerModel model)
        {
            _model = model;
        }

        public string Urls
        {
            get
            {
                return _model.Urls;
            }

            set
            {
                if (_model.Urls == value)
                    return;

                _model.Urls = value;
                OnPropertyChanged(nameof(Urls));
            }
        }

        public string SecretKey
        {
            get
            {
                return _model.SecretKey;
            }

            set
            {
                if (_model.SecretKey == value)
                    return;

                _model.SecretKey = value;
                OnPropertyChanged(nameof(SecretKey));
            }
        }

        public void RefreshModel()
        {
            OnPropertyChanged(nameof(Urls));
            OnPropertyChanged(nameof(SecretKey));
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
