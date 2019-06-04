using System.ComponentModel;

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

        private ConfigurationModel _model;

        public ConfigurationViewModel()
        {
            _model = new ConfigurationModel();

            AdminModel = new CredentialViewModel(_model.CredentialsManager.Admin);
            DealerModel = new CredentialViewModel(_model.CredentialsManager.Dealer);
            ViewerModel = new CredentialViewModel(_model.CredentialsManager.Viewer);

            SslModel = new SslViewModel(_model.SslManager.SslModel);
            ProtocolModel = new ProtocolViewModel(_model.ProtocolManager.ProtocolModel);
            ServerModel = new ServerViewModel(_model.ServerManager.ServerModel);
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
        }

        public void StartAgent()
        {
            _model.StartAgent();
        }
    }
}
