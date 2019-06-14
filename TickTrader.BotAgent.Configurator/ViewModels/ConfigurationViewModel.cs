using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace TickTrader.BotAgent.Configurator
{
    public class ConfigurationViewModel : IDisposable
    {
        public CredentialViewModel AdminModel { get; set; }

        public CredentialViewModel DealerModel { get; set; }

        public CredentialViewModel ViewerModel { get; set; }

        public SslViewModel SslModel { get; set; }

        public ProtocolViewModel ProtocolModel { get; set; }

        public ServerViewModel ServerModel { get; set; }

        public StateServiceViewModel StateServiceModel { get; set; }

        public FdkViewModel FdkModel { get; set; }

        public RefreshManager RefreshManager { get; }

        public bool WasUpdate => RefreshManager.Update;

        private ConfigurationModel _model;
        private bool _runnignApplication;

        public ConfigurationViewModel()
        {
            _runnignApplication = true;
            _model = new ConfigurationModel();
            RefreshManager = new RefreshManager();

            AdminModel = new CredentialViewModel(_model.CredentialsManager.Admin, RefreshManager);
            DealerModel = new CredentialViewModel(_model.CredentialsManager.Dealer, RefreshManager);
            ViewerModel = new CredentialViewModel(_model.CredentialsManager.Viewer, RefreshManager);

            SslModel = new SslViewModel(_model.SslManager.SslModel, RefreshManager);
            ProtocolModel = new ProtocolViewModel(_model.ProtocolManager.ProtocolModel, RefreshManager);
            ServerModel = new ServerViewModel(_model.ServerManager.ServerModel, RefreshManager);
            FdkModel = new FdkViewModel(_model.FdkManager.FdkModel, RefreshManager);

            StateServiceModel = new StateServiceViewModel(_model.Settings[AppProperties.ServiceName]);

            ThreadPool.QueueUserWorkItem(RefreshServiceState);
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
            FdkModel.RefreshModel();
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

        public void Dispose()
        {
            _runnignApplication = false;
        }

        private async void RefreshServiceState(object obj)
        {
            while (_runnignApplication)
            {
                StateServiceModel.RefreshService(_model.ServiceManager.ServiceStatus);
                await Task.Delay(4000);
            }
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

    public interface IViewModel : INotifyPropertyChanged
    {
        void RefreshModel();

        void OnPropertyChanged([CallerMemberName]string prop = "");
    }
}
