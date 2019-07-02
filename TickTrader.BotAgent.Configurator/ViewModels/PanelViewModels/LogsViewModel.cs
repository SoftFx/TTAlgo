using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TickTrader.BotAgent.Configurator
{
    public class LogsViewModel : INotifyPropertyChanged
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

            Logger.Info("Log was updated");

            OnPropertyChanged(nameof(Messages));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
