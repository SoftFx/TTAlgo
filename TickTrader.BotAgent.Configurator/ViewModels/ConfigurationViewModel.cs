using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace TickTrader.BotAgent.Configurator
{
    public class ConfigurationViewModel : IDisposable
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private List<BaseViewModel> _viewModels;

        private Window _mainWindow;

        private AgentVersionManager _versionManager;
        private ConfigurationModel _model;

        private bool _runnignApplication;

        private DelegateCommand _startAgent;
        private DelegateCommand _restartApplication;
        private DelegateCommand _saveChanges;
        private DelegateCommand _cancelChanges;
        private DelegateCommand _openLogsFolder;

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

        public SpinnerViewModel Spinner { get; }

        public bool WasUpdate => RefreshManager.Update;

        public ConfigurationViewModel()
        {
            try
            {
                _mainWindow = Application.Current.MainWindow;

                _runnignApplication = true;
                _model = new ConfigurationModel();

                RefreshManager = new RefreshManager();
                Spinner = new SpinnerViewModel();

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

                _viewModels = new List<BaseViewModel>() { AdminModel, DealerModel, ViewerModel, SslModel, ProtocolModel, ServerModel, FdkModel, AdvancedModel };

                RefreshManager.NewValuesEvent += () => StateServiceModel.VisibleRestartMessage = true;
                RefreshManager.SaveValuesEvent += () => StateServiceModel.VisibleRestartMessage = false;

                ThreadPool.QueueUserWorkItem(RefreshServiceState);

                _mainWindow.Title = $"BotAgent Configurator: {_versionManager.FullVersion}";
                _mainWindow.Closing += MainWindow_Closing;
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex);
                MessageBoxManager.ErrorBox(ex.Message);
                Application.Current.Shutdown();
            }
        }

        public DelegateCommand StartAgent => _startAgent ?? (
            _startAgent = new DelegateCommand(obj =>
            {
                if (WasUpdate)
                {
                    if (MessageBoxManager.OkCancelBoxQuestion("To start the agent, you need to save the new settings. Continue?", "Restart"))
                        SaveModelChanges();
                    else
                        return;
                }

                if (_model.ServiceManager.IsServiceRunning && !MessageBoxManager.OkCancelBoxQuestion("The current process will be restarted. Continue?", "Restart"))
                    return;

                Spinner.Run = true;
                ThreadPool.QueueUserWorkItem(StartAgentMethod);
            }));

        public DelegateCommand RestartApplication => _restartApplication ?? (
            _restartApplication = new DelegateCommand(obj =>
            {
                if (SaveChangesMethod() == MessageBoxResult.Cancel)
                    return;

                _mainWindow.Closing -= MainWindow_Closing;

                _logger.Info($"The application has been restarted!");
                Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }));

        public DelegateCommand SaveChanges => _saveChanges ?? (
            _saveChanges = new DelegateCommand(obj =>
            {
                try
                {
                    _model.SaveChanges();
                    RefreshManager.DropRefresh();
                    MessageBoxManager.OKBox("Configuration saved successfully!");
                    _logger.Info($"Changes have been saved.");
                }
                catch (Exception ex)
                {
                    MessageBoxManager.ErrorBox(ex.Message);
                    _logger.Error(ex);
                }
            }));

        public DelegateCommand CancelChanges => _cancelChanges ?? (
            _cancelChanges = new DelegateCommand(obj =>
            {
                if (MessageBoxManager.OkCancelBoxQuestion("Changes will be reset. Continue?", "Reset changes"))
                {
                    try
                    {
                        _model.LoadConfiguration();
                        RefreshManager.DropRefresh();

                        DropAllErrors();
                        RefreshModels();
                        _logger.Info($"Changes have been reset.");
                    }
                    catch (Exception ex)
                    {
                        _logger.Fatal(ex);
                        MessageBoxManager.ErrorBox(ex.Message);
                        Application.Current.Shutdown();
                    }
                }
            }));

        public DelegateCommand OpenLogsFolder => _openLogsFolder ?? (
            _openLogsFolder = new DelegateCommand(obj =>
            {
                Process.Start(Path.GetDirectoryName(_model.Logs.LogsFilePath));
            }));

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

            _logger.Info($"Models have been updated.");
        }

        public void Dispose()
        {
            _runnignApplication = false;
        }

        private void StartAgentMethod(object obj)
        {
            try
            {
                _model.StartAgent();

                Spinner.Run = false;
                MessageBoxManager.OKBox("Agent has been started!");
                _logger.Info($"Agent has been started!");
            }
            catch (WarningException ex)
            {
                _logger.Error(ex);
                MessageBoxManager.WarningBox(ex.Message);
            }
            catch (Exception exx)
            {
                _logger.Error(exx);
                MessageBoxManager.ErrorBox(exx.Message);
            }
            finally
            {
                Spinner.Run = false;
            }
        }

        private async void RefreshServiceState(object obj)
        {
            try
            {
                LogsModel = new LogsViewModel(_model.Logs);

                int sec = 0;

                while (_runnignApplication)
                {
                    StateServiceModel.RefreshService(_model.ServiceManager.ServiceStatus);

                    sec = sec == 5 ? 0 : sec + 1;

                    if (sec == 0)
                        LogsModel.RefreshLog();

                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private void SaveModelChanges()
        {
            if (!WasUpdate)
                return;

            _model.SaveChanges();
            RefreshManager.DropRefresh();
        }

        private MessageBoxResult SaveChangesMethod()
        {
            try
            {
                if (!WasUpdate || ModelErrorCounter.TotalErrorCount != 0)
                    return MessageBoxResult.No;

                var result = MessageBoxManager.YesNoCancelQuestion("Save new changes for current agent?", "New changes");

                if (result == MessageBoxResult.Yes)
                    SaveModelChanges();

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                MessageBoxManager.ErrorBox("Saved settings was failed");
                return MessageBoxResult.No;
            }
            finally
            {
                Dispose();
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (SaveChangesMethod() == MessageBoxResult.Cancel)
                e.Cancel = true;
        }

        private void DropAllErrors()
        {
            foreach (var model in _viewModels)
            {
                model.ErrorCounter.DropAll();
            }
        }
    }

    public class RefreshManager : BaseViewModel
    {
        private SortedSet<string> _updatedFields;

        public delegate void ConfigurationStateChanged();

        public event ConfigurationStateChanged NewValuesEvent;
        public event ConfigurationStateChanged SaveValuesEvent;

        public bool Update => _updatedFields.Count > 0;

        public RefreshManager()
        {
            _updatedFields = new SortedSet<string>();
        }

        public void CheckUpdate(string newValue, string oldValue, string field)
        {
            if (newValue != oldValue)
                AddUpdate(field);
            else
                DeleteUpdate(field);
        }

        public void AddUpdate(string field)
        {
            if (!_updatedFields.Contains(field))
                _updatedFields.Add(field);

            NewValuesEvent?.Invoke();
            OnPropertyChanged(nameof(Update));
        }

        public void DeleteUpdate(string field)
        {
            if (_updatedFields.Contains(field))
                _updatedFields.Remove(field);

            if (_updatedFields.Count == 0)
                DropRefresh();

            OnPropertyChanged(nameof(Update));
        }

        public void DropRefresh()
        {
            _updatedFields.Clear();

            SaveValuesEvent?.Invoke();
            OnPropertyChanged(nameof(Update));
        }
    }

    public interface IContentViewModel
    {
        string ModelDescription { get; set; }

        void RefreshModel();
    }
}
