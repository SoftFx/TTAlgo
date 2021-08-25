namespace TickTrader.BotAgent.Configurator
{
    public class AlgoServerSettingsViewModel : BaseContentViewModel
    {
        private readonly RefreshCounter _refreshManager;
        private readonly AlgoServerSettingsManager _manager;
        private readonly AlgoServerSettingsModel _model;
        private readonly string _devModeKey;


        public AlgoServerSettingsViewModel(AlgoServerSettingsManager manager, RefreshCounter refManager = null) : base(nameof(AlgoServerSettingsViewModel))
        {
            _manager = manager;
            _model = _manager.AlgoServerModel;
            _refreshManager = refManager;

            _devModeKey = $"{nameof(AlgoServerSettingsViewModel)} {nameof(EnableDevMode)}";
        }

        public bool EnablePanel => _manager.EnableManager;


        public bool EnableDevMode
        {
            get => _model.EnableDevMode;

            set
            {
                if (_model.EnableDevMode == value)
                    return;

                _model.EnableDevMode = value;
                _refreshManager?.CheckUpdate(value.ToString(), _model.CurrentEnableDevModel.ToString(), _devModeKey);

                OnPropertyChanged(nameof(EnableDevMode));
            }
        }

        public override void RefreshModel()
        {
            OnPropertyChanged(nameof(EnableDevMode));
            OnPropertyChanged(nameof(EnablePanel));
        }
    }
}
