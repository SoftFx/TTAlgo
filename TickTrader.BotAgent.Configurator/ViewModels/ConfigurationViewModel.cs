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
    public class ConfigurationViewModel : BaseViewModel, IDisposable
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private List<BaseContentViewModel> _viewModels;

        private Window _mainWindow;
        private ConfigurationModel _model;
        private AppInstanceRestrictor _appRestrictor = new AppInstanceRestrictor();

        private bool _runnignApplication;
        private string _title;

        private DelegateCommand _startAgent;
        private DelegateCommand _stopAgent;
        private DelegateCommand _restartApplication;
        private DelegateCommand _saveChanges;
        private DelegateCommand _cancelChanges;
        private DelegateCommand _openLogsFolder;

        public string Title
        {
            get => _title;

            set
            {
                if (_title == value)
                    return;

                _title = $"BotAgent Configurator: {value}";
                OnPropertyChanged(nameof(Title));
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

        public RefreshCounter RefreshCounter { get; }

        public SpinnerViewModel Spinner { get; }

        public bool WasUpdate => RefreshCounter.Update;

        public bool IsDeveloperVersion => _model.Settings.IsDeveloper;

        public bool ServiceRunning => _model?.ServiceManager.IsServiceRunning ?? false;

        public ConfigurationViewModel()
        {
            try
            {
                if (!_appRestrictor.EnsureSingleInstace())
                    Application.Current.Shutdown();

                _mainWindow = Application.Current.MainWindow;
                _model = new ConfigurationModel();

                RefreshCounter = new RefreshCounter();
                StateServiceModel = new StateServiceViewModel();
                Spinner = new SpinnerViewModel();

                SetNewViewModels();

                RefreshCounter.NewValuesEvent += () => StateServiceModel.VisibleRestartMessage = true;
                RefreshCounter.SaveValuesEvent += () => StateServiceModel.VisibleRestartMessage = false;

                _mainWindow.Closing += MainWindow_Closing;
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex);
                MessageBoxManager.OkError(ex.Message);

                _appRestrictor.Dispose();
                Dispose();

                Application.Current.Shutdown();
            }
        }

        private void SetNewViewModels()
        {
            Title = _model.CurrentAgent.FullVersion;
            LogsModel = new LogsViewModel(_model.Logs);

            AdminModel = new CredentialViewModel(_model.CredentialsManager.Admin, RefreshCounter)
            {
                ModelDescription = _model.Prompts.GetPrompt(SectionNames.Credentials, _model.CredentialsManager.Admin.Name)
            };

            DealerModel = new CredentialViewModel(_model.CredentialsManager.Dealer, RefreshCounter)
            {
                ModelDescription = _model.Prompts.GetPrompt(SectionNames.Credentials, _model.CredentialsManager.Dealer.Name)
            };

            ViewerModel = new CredentialViewModel(_model.CredentialsManager.Viewer, RefreshCounter)
            {
                ModelDescription = _model.Prompts.GetPrompt(SectionNames.Credentials, _model.CredentialsManager.Viewer.Name)
            };

            //SslModel = new SslViewModel(_model.SslManager.SslModel, RefreshCounter);

            ProtocolModel = new ProtocolViewModel(_model.ProtocolManager.ProtocolModel, RefreshCounter)
            {
                ListeningPortDescription = _model.Prompts.GetPrompt(SectionNames.Protocol, ProtocolManager.PortNameProperty),
                DirectoryNameDescription = _model.Prompts.GetPrompt(SectionNames.Protocol, ProtocolManager.DirectoryNameProperty),
                LogMessageDescription = _model.Prompts.GetPrompt(SectionNames.Protocol, ProtocolManager.UseLogNameProperty),
            };

            ServerModel = new ServerViewModel(_model.ServerManager.ServerModel, RefreshCounter)
            {
                UrlsDescription = _model.Prompts.GetPrompt(SectionNames.Server, ServerManager.UrlsNameProperty),
                SecretKeyDescription = _model.Prompts.GetPrompt(SectionNames.Server, ServerManager.SecretKeyNameProperty),
            };

            FdkModel = new FdkViewModel(_model.FdkManager.FdkModel, RefreshCounter)
            {
                ModelDescription = _model.Prompts.GetPrompt(SectionNames.Fdk, FdkManager.EnableLogsNameProperty),
            };

            AdvancedModel = new AdvancedViewModel(_model.RegistryManager, RefreshCounter)
            {
                ModelDescription = _model.Prompts.GetPrompt(SectionNames.MultipleAgentProvider, RegistryManager.AgentPathNameProperty),
            };

            _viewModels = new List<BaseContentViewModel>() { AdminModel, DealerModel, ViewerModel, ProtocolModel, ServerModel, FdkModel, AdvancedModel };
            _runnignApplication = true;

            ThreadPool.QueueUserWorkItem(RefreshServiceState);
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

                ThreadPool.QueueUserWorkItem(StartAndStopAgentMethod, true);
            }));

        public DelegateCommand StopAgent => _stopAgent ?? (
            _stopAgent = new DelegateCommand(obj =>
            {
                if (ServiceRunning && MessageBoxManager.OkCancelBoxQuestion("The current agent will be stopped. Continue?", "Stop"))
                {
                    ThreadPool.QueueUserWorkItem(StartAndStopAgentMethod, false);
                }
            }));

        public DelegateCommand RestartApplication => _restartApplication ?? (
            _restartApplication = new DelegateCommand(obj =>
            {
                if (SaveChangesMethod() == MessageBoxResult.Cancel)
                    return;

                Spinner.Start();

                DropAllErrors();
                RefreshCounter.DropRefresh();
                LogsModel.DropLog();

                _model.RefreshModel(AdvancedModel.SelectPath);
                SetNewViewModels();

                RefreshModels();
                _logger.Info($"The application has been restarted!");

                Spinner.Stop();
                MessageBoxManager.OKBox($"Agent {AdvancedModel.SelectPath} has been loaded");
            }));

        public DelegateCommand SaveChanges => _saveChanges ?? (
            _saveChanges = new DelegateCommand(obj =>
            {
                try
                {
                    _model.SaveChanges();
                    RefreshCounter.DropRefresh();
                    _logger.Info($"Changes have been saved.");
                    MessageBoxManager.OKBox("Configuration saved successfully!");
                }
                catch (Exception ex)
                {
                    MessageBoxManager.OkError(ex.Message);
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
                        DropAllErrors();
                        RefreshCounter.DropRefresh();

                        _model.LoadConfiguration();

                        RefreshAllViewModels();

                        _logger.Info($"Changes have been reset.");
                    }
                    catch (Exception ex)
                    {
                        _logger.Fatal(ex);
                        MessageBoxManager.OkError(ex.Message);
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
            OnPropertyChanged(nameof(AdminModel));
            OnPropertyChanged(nameof(DealerModel));
            OnPropertyChanged(nameof(ViewerModel));

            OnPropertyChanged(nameof(ProtocolModel));
            OnPropertyChanged(nameof(ServerModel));
            OnPropertyChanged(nameof(FdkModel));
            OnPropertyChanged(nameof(AdvancedModel));

            OnPropertyChanged(nameof(LogsModel));
            OnPropertyChanged(nameof(StateServiceModel));
            OnPropertyChanged(nameof(ServiceRunning));

            _logger.Info($"Models have been updated.");
        }

        public void Dispose()
        {
            _runnignApplication = false;
        }

        private void StartAndStopAgentMethod(object start)
        {
            try
            {
                Spinner.Start();

                if ((bool)start)
                    _model.StartAgent();
                else
                    _model.StopAgent();

                var mes = (bool)start ? $"Agent has been started!" : $"Agent has been stopped!";

                _logger.Info(mes);

                Spinner.Stop();
                OnPropertyChanged(nameof(ServiceRunning));

                MessageBoxManager.OKBox(mes);
            }
            catch (WarningException ex)
            {
                _logger.Error(ex);
                MessageBoxManager.WarningBox(ex.Message);
            }
            catch (Exception exx)
            {
                _logger.Error(exx);
                MessageBoxManager.OkError(exx.Message);
            }
            finally
            {
                Spinner.Stop();
            }
        }

        private async void RefreshServiceState(object obj)
        {
            try
            {
                int sec = 0;

                while (_runnignApplication)
                {
                    StateServiceModel.RefreshService(_model.ServiceManager.ServiceDisplayName, _model.ServiceManager.ServiceStatus);

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
            RefreshCounter.DropRefresh();
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
                MessageBoxManager.OkError("Saved settings was failed");
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

        private void RefreshAllViewModels()
        {
            foreach (var model in _viewModels)
            {
                model.RefreshModel();
            }
        }
    }
}
