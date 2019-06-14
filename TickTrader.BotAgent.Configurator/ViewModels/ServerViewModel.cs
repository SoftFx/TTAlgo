using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TickTrader.BotAgent.Configurator
{
    public class ServerViewModel : IViewModel
    {
        private ServerModel _model;
        private RefreshManager _refreshManager;

        public ServerViewModel(ServerModel model, RefreshManager refManager = null)
        {
            _model = model;
            _refreshManager = refManager;
        }

        public string Urls
        {
            get => _model.Urls;

            set
            {
                if (_model.Urls == value)
                    return;

                _model.Urls = value;
                _refreshManager?.Refresh();

                OnPropertyChanged(nameof(Urls));
            }
        }

        public string SecretKey
        {
            get => _model.SecretKey;

            set
            {
                if (_model.SecretKey == value)
                    return;

                _model.SecretKey = value;
                _refreshManager?.Refresh();

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
