using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace TickTrader.BotAgent.Configurator
{
    public class ConfigurationViewModel : INotifyPropertyChanged, IDisposable
    {
        private bool _visibleRestartMessage;
        private Window _mainWindow;

        private AgentVersionManager _versionManager;

        public string RestartMessage => "To apply the new settings, restart the service.";

        public bool VisibleRestartMessage
        {
            get => _visibleRestartMessage;
            set
            {
                if (_visibleRestartMessage == value)
                    return;

                _visibleRestartMessage = value;

                OnPropertyChanged(nameof(VisibleRestartMessage));
            }
        }

        public CredentialViewModel AdminModel { get; set; }

        public CredentialViewModel DealerModel { get; set; }

        public CredentialViewModel ViewerModel { get; set; }

        public SslViewModel SslModel { get; set; }

        public ProtocolViewModel ProtocolModel { get; set; }

        public ServerViewModel ServerModel { get; set; }

        public StateServiceViewModel StateServiceModel { get; set; }

        public FdkViewModel FdkModel { get; set; }

        public AdvancedViewModel AdvancedModel { get; set; }

        public LogsViewModel LogsModel { get; set; }

        public RefreshManager RefreshManager { get; }

        public bool WasUpdate => RefreshManager.Update;

        private ConfigurationModel _model;
        private bool _runnignApplication;

        public event PropertyChangedEventHandler PropertyChanged;

        public ConfigurationViewModel()
        {
            _mainWindow = Application.Current.MainWindow;

            _runnignApplication = true;
            _model = new ConfigurationModel();

            RefreshManager = new RefreshManager();
            RefreshManager.NewValuesEvent += () => VisibleRestartMessage = true;
            RefreshManager.SaveValuesEvent += () => VisibleRestartMessage = false;

            _versionManager = new AgentVersionManager(_model.BotAgentHolder.BotAgentPath, _model.Settings[AppProperties.ApplicationName]);

            AdminModel = new CredentialViewModel(_model.CredentialsManager.Admin, RefreshManager)
            {
                ModelDescription = _model.Prompts.GetPrompt(SectionNames.Credentials, _model.CredentialsManager.Admin.Name)
            };

            DealerModel = new CredentialViewModel(_model.CredentialsManager.Dealer, RefreshManager)
            {
                ModelDescription = _model.Prompts.GetPrompt(SectionNames.Credentials, _model.CredentialsManager.Dealer.Name)
            };

            ViewerModel = new CredentialViewModel(_model.CredentialsManager.Viewer, RefreshManager)
            {
                ModelDescription = _model.Prompts.GetPrompt(SectionNames.Credentials, _model.CredentialsManager.Viewer.Name)
            };


            SslModel = new SslViewModel(_model.SslManager.SslModel, RefreshManager);

            ProtocolModel = new ProtocolViewModel(_model.ProtocolManager.ProtocolModel, RefreshManager)
            {
                ListeningPortDescription = _model.Prompts.GetPrompt(SectionNames.Protocol, ProtocolManager.PortNameProperty),
                DirectoryNameDescription = _model.Prompts.GetPrompt(SectionNames.Protocol, ProtocolManager.DirectoryNameProperty),
                LogMessageDescription = _model.Prompts.GetPrompt(SectionNames.Protocol, ProtocolManager.UseLogNameProperty),
            };

            ServerModel = new ServerViewModel(_model.ServerManager.ServerModel, RefreshManager)
            {
                UrlsDescription = _model.Prompts.GetPrompt(SectionNames.Server, ServerManager.UrlsNameProperty),
                SecretKeyDescription = _model.Prompts.GetPrompt(SectionNames.Server, ServerManager.SecretKeyNameProperty),
            };

            FdkModel = new FdkViewModel(_model.FdkManager.FdkModel, RefreshManager)
            {
                ModelDescription = _model.Prompts.GetPrompt(SectionNames.Fdk, FdkManager.EnableLogsNameProperty),
            };


            AdvancedModel = new AdvancedViewModel(_model.Settings, RefreshManager)
            {
                ModelDescription = _model.Prompts.GetPrompt(SectionNames.MultipleAgentProvider, MultipleAgentConfigurator.AgentCongPathsNameSection),
            };

            StateServiceModel = new StateServiceViewModel(_model.Settings[AppProperties.ServiceName]);
            LogsModel = new LogsViewModel(_model.Logs);


            ThreadPool.QueueUserWorkItem(RefreshServiceState);

            _mainWindow.Title = $"BotAgent Configurator: {_versionManager.FullVersion}";
            //Application.Current.MainWindow.Closin
        }

        public void CancelChanges()
        {
            _model.LoadConfiguration();
            RefreshManager.DropRefresh();
            RefreshModels();
        }

        public void RefreshModels()
        {
            ServerModel.ResetSetting();

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

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private async void RefreshServiceState(object obj)
        {
            while (_runnignApplication)
            {
                StateServiceModel.RefreshService(_model.ServiceManager.ServiceStatus);
                LogsModel.RefreshLog();
                await Task.Delay(1000);
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

    public interface IContentViewModel : INotifyPropertyChanged
    {
        string ModelDescription { get; set; }

        void RefreshModel();

        void OnPropertyChanged([CallerMemberName]string prop = "");
    }
}
