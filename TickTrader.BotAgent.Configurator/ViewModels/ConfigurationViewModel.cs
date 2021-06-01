using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TickTrader.BotAgent.Configurator.Properties;
using TickTrader.WpfWindowsSupportLibrary;

namespace TickTrader.BotAgent.Configurator
{
    public class ConfigurationViewModel : BaseViewModel, IDisposable
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private List<BaseContentViewModel> _viewModels;

        private Window _mainWindow;
        private ConfigurationModel _model;
        private AppInstanceRestrictor _appRestrictor = new AppInstanceRestrictor(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "applock"));

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

                _title = $"AlgoServer Configurator: {value}";
                OnPropertyChanged(nameof(Title));
            }
        }

        public CredentialViewModel AdminModel { get; set; }

        public CredentialViewModel DealerModel { get; set; }

        public CredentialViewModel ViewerModel { get; set; }

        public SslViewModel SslModel { get; set; }

        public ProtocolViewModel ProtocolModel { get; set; }

        public ServerViewModel ServerModel { get; set; }

        public ServerBotSettingsViewModel ServerBotSettingsModel { get; set; }

        public StateServiceViewModel StateServiceModel { get; set; }

        public FdkViewModel FdkModel { get; set; }

        public AdvancedViewModel AdvancedModel { get; set; }

        public LogsViewModel LogsModel { get; set; }

        public RefreshCounter RefreshCounter { get; }

        public SpinnerViewModel Spinner { get; }


        public bool WasUpdate => RefreshCounter.Update;


        public ConfigurationViewModel()
        {
            try
            {
                if (!_appRestrictor.EnsureSingleInstace())
                    Application.Current.Shutdown();

                _mainWindow = Application.Current.MainWindow;
                _model = new ConfigurationModel();

                Title = _model.CurrentAgent.FullVersion;

                RefreshCounter = new RefreshCounter();
                StateServiceModel = new StateServiceViewModel(RefreshCounter);
                Spinner = new SpinnerViewModel();

                SetNewViewModels();

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

            ServerModel = new ServerViewModel(_model.ServerManager.ServerModel, ProtocolModel, RefreshCounter)
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

            ServerBotSettingsModel = new ServerBotSettingsViewModel(_model.BotSettingsManager, Spinner)
            {
                ModelDescription = _model.Prompts.GetPrompt(SectionNames.MultipleAgentProvider, ServerBotSettingsManager.ServerBotSettingsProperty),
            };

            _viewModels = new List<BaseContentViewModel>() { AdminModel, DealerModel, ViewerModel, ProtocolModel, ServerModel, FdkModel, AdvancedModel };
            _runnignApplication = true;

            ProtocolModel.ChangeListeningPortEvent += () => ServerModel.RefreshModel();

            ThreadPool.QueueUserWorkItem(RefreshServiceState);
        }

        public DelegateCommand StartAgent => _startAgent ?? (
            _startAgent = new DelegateCommand(obj =>
            {
                if (WasUpdate)
                {
                    if (MessageBoxManager.OkCancelBoxQuestion(Resources.SaveBeforeStartQuestion, Resources.RestartTitle))
                        SaveModelChanges();
                    else
                        return;
                }

                if (_model.ServiceManager.IsServiceRunning && !MessageBoxManager.OkCancelBoxQuestion(Resources.RestartProcessQuestion, Resources.RestartTitle))
                    return;

                ThreadPool.QueueUserWorkItem(StartAndStopAgentMethod, true);
            }));

        public DelegateCommand StopAgent => _stopAgent ?? (
            _stopAgent = new DelegateCommand(obj =>
            {
                if (StateServiceModel.ServiceRunning && MessageBoxManager.OkCancelBoxQuestion(Resources.StopAgentQuestion, Resources.StopTitle))
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
                LogsModel.DropLog();

                _model.RefreshModel(AdvancedModel.SelectPath);
                SetNewViewModels();

                RefreshModels();
                _logger.Info(Resources.RestartAppLog);

                Spinner.Stop();
                StateServiceModel.InfoMessage = $"{Resources.SelectAgentMes_} {AdvancedModel.SelectPath} {Resources._SelectAgentMes}";
            }));

        public DelegateCommand SaveChanges => _saveChanges ?? (
            _saveChanges = new DelegateCommand(obj =>
            {
                try
                {
                    RefreshCounter.DropRefresh();
                    StateServiceModel.RestartRequired = true;
                    _model.SaveChanges();

                    _logger.Info(Resources.SaveChangesLog);

                    StateServiceModel.InfoMessage = Resources.SuccessfullySavedMes;
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
                if (MessageBoxManager.OkCancelBoxQuestion(Resources.ResetChangesQuestion, Resources.ResetChangesTitle))
                {
                    try
                    {
                        DropAllErrors();

                        _model.LoadConfiguration();

                        RefreshAllViewModels();

                        _logger.Info(Resources.ResetChangesLog);
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
                string path = Path.GetDirectoryName(_model.Logs.LogsFilePath);

                if (Directory.Exists(path))
                    Process.Start(path);
                else
                    MessageBoxManager.OkError($"{path} folder path not found");
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

            _logger.Info(Resources.UpdateModelsLog);
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
                {
                    RefreshCounter.DropRestart();

                    _model.StartAgent();
                }
                else
                    _model.StopAgent();

                _logger.Info((bool)start ? Resources.StartAgentLog : Resources.StopAgentLog);

                Spinner.Stop();
            }
            catch (WarningException ex)
            {
                _logger.Warn(ex);
                Application.Current.Dispatcher.BeginInvoke(new Action<string>(MessageBoxManager.OkWarningBox), ex.Message);
            }
            catch (Exception exx)
            {
                _logger.Error(exx);
                Application.Current.Dispatcher.BeginInvoke(new Action<string>(MessageBoxManager.OkError), exx.Message);
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
                    StateServiceModel.RefreshService(_model.ServiceManager.MachineServiceName, _model.ServiceManager.ServiceDisplayName);

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

                var result = MessageBoxManager.YesNoCancelQuestion(Resources.SaveChangesQuestion, Resources.NewChangesTitle);

                if (result == MessageBoxResult.Yes)
                    SaveModelChanges();

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                MessageBoxManager.OkError(Resources.SaveIsFailedEx);
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

            RefreshCounter.AllDrop();
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
