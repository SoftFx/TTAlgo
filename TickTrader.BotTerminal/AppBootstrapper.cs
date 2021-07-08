using ActorSharp;
using Caliburn.Micro;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TickTrader.Algo.Account;
using TickTrader.Algo.Account.Fdk2;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.CoreV1;
using TickTrader.Algo.Isolation.NetFx;
using TickTrader.Algo.Logging;
using TickTrader.Algo.Package;
using TickTrader.Algo.Server;
using TickTrader.Algo.ServerControl;
using TickTrader.FeedStorage;
using TickTrader.WpfWindowsSupportLibrary;

namespace TickTrader.BotTerminal
{
    public class AppBootstrapper : BootstrapperBase
    {
        private static readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly AutoViewManager autoViewLocator = new AutoViewManager();

        public static CultureInfo CultureCache { get; private set; }

        private AppInstanceRestrictor _instanceRestrictor = new AppInstanceRestrictor(EnvService.Instance.AppLockFilePath);
        private SimpleContainer _container = new SimpleContainer();
        private ShellViewModel _shell;
        private bool _hasWriteAccess;

        public AppBootstrapper()
        {
            CultureCache = CultureInfo.CurrentCulture;

            //CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            //CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            LocaleSelector.Instance.ActivateDefault();

            Initialize();

            _hasWriteAccess = HasWriteAccess();
            if (_hasWriteAccess)
            {
                ConfigureCaliburn();
                ConfigurateLogger();
                ConfigureGlobalExceptionHandling();

                PackageLoadContext.Init(PackageLoadContextProvider.Create);
                PackageExplorer.Init(PackageV1Explorer.Create());
                PluginLogWriter.Init(NLogPluginLogWriter.Create);
            }
        }

        public static AutoViewManager AutoViewLocator => autoViewLocator;

        private void ConfigureCaliburn()
        {
            ViewLocator.AddDefaultTypeMapping("Page");
            ViewLocator.AddDefaultTypeMapping("Dialog");

            ViewLocator.LocateForModelType = (modelType, displayLocation, context) =>
            {
                var viewType = ViewLocator.LocateTypeForModelType(modelType, displayLocation, context);

                if (viewType == null)
                    return autoViewLocator.CreateView(modelType, context);

                return ViewLocator.GetOrCreateViewType(viewType);
            };

            MessageBinder.SpecialValues.Add("$password", context =>
            {
                var view = (FrameworkElement)context.View;
                var pwd = view.FindName("PasswordInput") as System.Windows.Controls.PasswordBox;

                if (pwd == null)
                    throw new Exception("To use $password you should have PasswordBox named 'PasswordInput' on your View.");

                return pwd.Password;
            });

            MessageBinder.SpecialValues.Add("$originalsourcecontext", context =>
            {
                var args = context.EventArgs as RoutedEventArgs;
                if (args == null)
                {
                    return null;
                }

                var fe = args.OriginalSource as FrameworkElement;
                if (fe == null)
                {
                    return null;
                }

                return fe.DataContext;
            });

            MessageBinder.SpecialValues.Add("$tag", context =>
            {
                return context.Source.Tag;
            });
        }

        private void ConfigureGlobalExceptionHandling()
        {
            if (System.Diagnostics.Debugger.IsAttached)
                return;

            Application.DispatcherUnhandledException += (s, e) =>
            {
                e.Handled = true;
                logger.Error(e.Exception, "Unhandled Exception on Dispatcher level!");
            };

            Actor.UnhandledException += (e) =>
            {
                logger.Error(e, "Unhandled Exception on Actor level!");
            };
        }

        private void ConfigurateLogger()
        {
            var config = new LoggingConfiguration();

            var p = new NLogFileParams { LogDirectory = EnvService.Instance.JournalFolder, Layout = NLogHelper.SimpleLogLayout };

            p.FileNameSuffix = "journal";
            var journalTarget = NLogHelper.CreateAsyncFileTarget(p, 500, 10000);
            config.AddRule(LogLevel.Trace, LogLevel.Trace, journalTarget, string.Concat("*", nameof(EventJournal)), true);

            p.LogDirectory = EnvService.Instance.LogFolder;

            p.FileNameSuffix = "alert";
            var alertTarget = NLogHelper.CreateAsyncFileTarget(p, 100, 1000);
            config.AddRule(LogLevel.Trace, LogLevel.Trace, alertTarget, string.Concat("*", nameof(AlertViewModel)), true);

            p.Layout = NLogHelper.NormalLogLayout;

            p.FileNameSuffix = "terminal";
            var logTarget = NLogHelper.CreateAsyncFileTarget(p, 500, 10000);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logTarget);

            p.FileNameSuffix = "terminal-error";
            var errTarget = NLogHelper.CreateAsyncFileTarget(p, 200, 1000);
            config.AddRule(LogLevel.Error, LogLevel.Fatal, errTarget);

            NLog.LogManager.Configuration = config;

            AlgoLoggerFactory.Init(NLogLoggerAdapter.Create);
            NonBlockingFileCompressor.Setup();
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            base.OnExit(sender, e);

            NLog.LogManager.Shutdown();
        }

        protected override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected async override void OnStartup(object sender, StartupEventArgs e)
        {
            if (!_hasWriteAccess || EnvService.Instance.InitFailed)
            {
                Application.Current.Shutdown();
                return;
            }

            if (!_instanceRestrictor.EnsureSingleInstace())
                Application.Current.Shutdown();
            else
            {
                CertificateProvider.InitServer(SslImport.LoadServerCertificate(), SslImport.LoadServerPrivateKey());

                var connectionOptions = new ConnectionOptions()
                {
                    AutoReconnect = true,
                    EnableLogs = BotTerminal.Properties.Settings.Default.EnableConnectionLogs,
                    LogsFolder = EnvService.Instance.LogFolder,
                    Type = AppType.BotTerminal,
                };

                var clientHandler = new ClientModel.ControlHandler((options, loggerId) => new SfxInterop(options, loggerId),connectionOptions, EnvService.Instance.FeedHistoryCacheFolder, FeedHistoryFolderOptions.ServerHierarchy, "0");
                var dataHandler = clientHandler.CreateDataHandler();
                await dataHandler.Init();

                var customStorage = new CustomFeedStorage.Handler(Actor.SpawnLocal<CustomFeedStorage>());
                await customStorage.SyncData();
                await customStorage.Start(EnvService.Instance.CustomFeedCacheFolder);

                _container.RegisterInstance(typeof(ClientModel.Data), null, dataHandler);
                _container.RegisterInstance(typeof(CustomFeedStorage.Handler), null, customStorage);
                _container.Singleton<IWindowManager, Caliburn.Micro.WindowManager>();
                _container.Singleton<IEventAggregator, EventAggregator>();
                _container.Singleton<ShellViewModel>();

                _shell = _container.GetInstance<ShellViewModel>();
                _shell.Deactivated += Shell_Deactivated; ;

                await DisplayRootViewFor<ShellViewModel>();
            }
        }

        private async Task Shell_Deactivated(object sender, DeactivationEventArgs e)
        {
            if (e.WasClosed)
            {
                await _shell.Shutdown();
                _instanceRestrictor.Dispose();
                App.Current.Shutdown();
            }
        }

        //private async void Shell_Deactivated(object sender, DeactivationEventArgs e)
        //{
        //    if (e.WasClosed)
        //    {
        //        await _shell.Shutdown();
        //        _instanceRestrictor.Dispose();
        //        App.Current.Shutdown();
        //    }
        //}

        private bool HasWriteAccess()
        {
            if (Execute.InDesignMode)
                return true;

            try
            {
                using (var file = new FileStream(EnvService.Instance.AppLockFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read)) { }
            }
            catch (UnauthorizedAccessException)
            {
                var res = AppAccessRightsElevator.ElevateToAdminRights();
                switch (res)
                {
                    case AccessElevationStatus.AlreadyThere:
                        MessageBox.Show($"Don't have access to write in directory {Directory.GetCurrentDirectory()}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    case AccessElevationStatus.Launched:
                    case AccessElevationStatus.UserCancelled:
                    default:
                        return false;
                }
            }
            catch (IOException) { /* Ignore locked files */ }
            return true;
        }
    }
}
