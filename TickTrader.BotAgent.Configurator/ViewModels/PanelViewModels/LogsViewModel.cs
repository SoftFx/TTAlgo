using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TickTrader.BotAgent.Configurator
{
    public class LogsViewModel
    {
        private LogsManager _manager;

        public LogsViewModel(LogsManager manager)
        {
            _manager = manager;
        }

        public string Messages => _manager.LogsStr;

        public void RefreshLog()
        {
            _manager.UpdateLog();

            OnPropertyChanged(nameof(Messages));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
