namespace TickTrader.BotAgent.Configurator
{
    public class FdkViewModel : BaseContentViewModel
    {
        private readonly RefreshCounter _refreshManager;
        private readonly string _keyLogs;

        private FdkModel _model;

        public FdkViewModel(FdkModel model, RefreshCounter refManager = null) : base(nameof(FdkViewModel))
        {
            _model = model;
            _refreshManager = refManager;

            _keyLogs = $"{nameof(FdkViewModel)} {nameof(EnableLogs)}";
        }

        public bool EnableLogs
        {
            get => _model.EnableLogs;
            set
            {
                if (_model.EnableLogs == value)
                    return;

                _model.EnableLogs = value;
                _refreshManager?.CheckUpdate(value.ToString(), _model.CurrentEnableLogs.ToString(), _keyLogs);

                OnPropertyChanged(nameof(EnableLogs));
            }
        }

        public override void RefreshModel()
        {
            OnPropertyChanged(nameof(EnableLogs));
        }
    }
}
