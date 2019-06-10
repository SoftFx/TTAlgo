namespace TickTrader.BotAgent.Configurator
{
    public class ConfigurationViewModel
    {
        public CredentialViewModel AdminModel { get; set; }

        public CredentialViewModel DealerModel { get; set; }

        public CredentialViewModel ViewerModel { get; set; }

        public SslViewModel SslModel { get; set; }

        public ProtocolViewModel ProtocolModel { get; set; }

        public ServerViewModel ServerModel { get; set; }

        public RefreshManager RefreshManager { get; }

        public bool WasUpdate => RefreshManager.Update;

        private ConfigurationModel _model;

        public ConfigurationViewModel()
        {
            _model = new ConfigurationModel();
            RefreshManager = new RefreshManager();

            AdminModel = new CredentialViewModel(_model.CredentialsManager.Admin, RefreshManager);
            DealerModel = new CredentialViewModel(_model.CredentialsManager.Dealer, RefreshManager);
            ViewerModel = new CredentialViewModel(_model.CredentialsManager.Viewer, RefreshManager);

            SslModel = new SslViewModel(_model.SslManager.SslModel, RefreshManager);
            ProtocolModel = new ProtocolViewModel(_model.ProtocolManager.ProtocolModel, RefreshManager);
            ServerModel = new ServerViewModel(_model.ServerManager.ServerModel, RefreshManager);
        }

        public void CancelChanges()
        {
            _model.LoadConfiguration();
            RefreshModels();
        }

        public void RefreshModels()
        {
            AdminModel.RefreshModel();
            DealerModel.RefreshModel();
            ViewerModel.RefreshModel();

            SslModel.RefreshModel();
            ProtocolModel.RefreshModel();
            ServerModel.RefreshModel();
        }

        public void SaveChanges()
        {
            _model.SaveChanges();
            RefreshManager.DropRefresh();
        }

        public bool StartAgent()
        {
            return _model.StartAgent();
        }
    }

    public class RefreshManager
    {
        public delegate void ConfigurationStateChanged();

        public event ConfigurationStateChanged NewValuesEvent;
        public event ConfigurationStateChanged SaveValuesEvent;

        public bool Update { get; private set; }

        public void Refresh()
        {
            Update = true;

            NewValuesEvent?.Invoke();
        }

        public void DropRefresh()
        {
            Update = false;

            SaveValuesEvent?.Invoke();
        }
    }
}
