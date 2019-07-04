namespace TickTrader.BotAgent.Configurator
{
    public class FdkViewModel : BaseViewModel, IContentViewModel
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

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

                _logger.Info(GetChangeMessage($"{nameof(FdkViewModel)} {nameof(EnableLogs)}", _model.EnableLogs.ToString(), value.ToString()));

                _model.EnableLogs = value;
                _refreshManager?.Refresh();

                OnPropertyChanged(nameof(EnableLogs));
            }
        }

        public string ModelDescription { get; set; }

        public void RefreshModel()
        {
            OnPropertyChanged(nameof(EnableLogs));
        }
    }
}
