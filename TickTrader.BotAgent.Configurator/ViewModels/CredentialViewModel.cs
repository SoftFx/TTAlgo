using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotAgent.Configurator
{
    public class CredentialViewModel : INotifyPropertyChanged
    {
        private CredentialModel _model;

        public CredentialViewModel(CredentialModel model)
        {
            _model = model;
        }

        public string Name => _model.Name;

        public string Login
        {
            get
            {
                return _model.Login;
            }
            set
            {
                if (_model.Login == value)
                    return;

                _model.Login = value;
                OnPropertyChanged(nameof(Login));
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

        public void GenerateNewPassword()
        {
            _model.GeneratePassword();

            OnPropertyChanged(nameof(Password));
        }

        public void RefreshModel()
        {
            OnPropertyChanged(nameof(Login));
            OnPropertyChanged(nameof(Password));
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
