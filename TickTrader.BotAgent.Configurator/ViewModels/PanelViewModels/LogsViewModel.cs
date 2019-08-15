namespace TickTrader.BotAgent.Configurator
{
    public class LogsViewModel : BaseViewModel
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private LogsManager _manager;

        public LogsViewModel(LogsManager manager)
        {
            _manager = manager;
        }

        public string Messages => _manager.LogsStr;

        public void RefreshLog()
        {
            _manager.UpdateLog();

            //_logger.Info("Log was updated");

            OnPropertyChanged(nameof(Messages));
        }

        public void DropLog()
        {
            _manager.DropLog();
            OnPropertyChanged(nameof(Messages));
        }
    }
}
