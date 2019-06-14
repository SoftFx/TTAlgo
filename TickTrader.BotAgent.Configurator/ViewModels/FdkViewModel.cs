using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TickTrader.BotAgent.Configurator
{
    public class FdkViewModel : IViewModel
    {
        private FdkModel _model;
        private RefreshManager _refreshManager;

        public FdkViewModel(FdkModel model, RefreshManager refManager = null)
        {
            _model = model;
            _refreshManager = refManager;
        }

        public bool EnableLogs
        {
            get => _model.EnableLogs;
            set
            {
                if (_model.EnableLogs == value)
                    return;

                _model.EnableLogs = value;
                _refreshManager?.Refresh();

                OnPropertyChanged(nameof(EnableLogs));
            }
        }

        public void RefreshModel()
        {
            OnPropertyChanged(nameof(EnableLogs));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

    }
}
