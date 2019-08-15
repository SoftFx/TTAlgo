namespace TickTrader.BotAgent.Configurator
{
    public class SslViewModel : BaseContentViewModel
    {
        private SslModel _model;
        private RefreshCounter _refreshManager;

        public SslViewModel(SslModel model, RefreshCounter refManager = null) : base(nameof(ServerViewModel))
        {
            _model = model;
            _refreshManager = refManager;
        }

        public string File
        {
            get => _model.File;

            set
            {
                if (_model.File == value)
                    return;

                _model.File = value;
                //_refreshManager?.Refresh();

                OnPropertyChanged(nameof(File));
            }
        }

        public string Password
        {
            get => _model.Password;

            set
            {
                if (_model.Password == value)
                    return;

                _model.Password = value;
                //_refreshManager?.Refresh();

                OnPropertyChanged(nameof(Password));
            }
        }

        public override void RefreshModel()
        {
            OnPropertyChanged(nameof(File));
            OnPropertyChanged(nameof(Password));
        }
    }
}
