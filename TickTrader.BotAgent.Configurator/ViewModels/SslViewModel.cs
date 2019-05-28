using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotAgent.Configurator
{
    public class SslViewModel : INotifyPropertyChanged
    {
        private SslModel _model;

        public SslViewModel(SslModel model)
        {
            _model = model;
        }

        public string File
        {
            get
            {
                return _model.File;
            }

            set
            {
                if (_model.File == value)
                    return;

                _model.File = value;
                OnPropertyChanged(nameof(File));
            }
        }

        public string Password
        {
            get
            {
                return _model.Password;
            }
            set
            {
                if (_model.Password == value)
                    return;

                _model.Password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public void RefreshModel()
        {
            OnPropertyChanged(nameof(File));
            OnPropertyChanged(nameof(Password));
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
