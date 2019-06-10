using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TickTrader.BotAgent.Configurator
{
    public class SslViewModel : INotifyPropertyChanged
    {
        private SslModel _model;
        private RefreshManager _refreshManager;

        public SslViewModel(SslModel model, RefreshManager refManager = null)
        {
            _model = model;
            _refreshManager = refManager;
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
                _refreshManager?.Refresh();

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
                _refreshManager?.Refresh();

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
