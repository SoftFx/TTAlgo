﻿using Caliburn.Micro;
using NLog;
using NLog.Config;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using TickTrader.Algo.Account;
using TickTrader.Algo.Account.Fdk2;
using TickTrader.Algo.Account.Settings;
using TickTrader.Algo.AppCommon;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Logging;
using TickTrader.Algo.PkgLoader;
using TickTrader.Algo.Server;
using TickTrader.Algo.Server.Common;
using TickTrader.SeriesStorage;
using TickTrader.SeriesStorage.Lmdb;
using TickTrader.WpfWindowsSupportLibrary;

namespace TickTrader.BotTerminal
{
    public class AppBootstrapper : BootstrapperBase
    {
        private static readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly SimpleContainer _container = new();
        private readonly AppInstanceRestrictor _instanceRestrictor;
        private readonly bool _hasWriteAccess;

        private ShellViewModel _shell;


        public static CultureInfo CultureCache { get; } = CultureInfo.CurrentCulture;

        public static AutoViewManager AutoViewLocator { get; } = new();


        public AppBootstrapper()
        {
            LocaleSelector.Instance.ActivateDefault();
            Initialize();

            ResolveAppInfo();

            _instanceRestrictor = new(EnvService.Instance.AppLockFilePath);
            _hasWriteAccess = HasWriteAccess();
            if (_hasWriteAccess)
            {
                ConfigureCaliburn();
                if (Execute.InDesignMode)
                    return;

                ConfigurateLogger();
                ConfigureGlobalExceptionHandling();
                logger.Info(AppInfoProvider.GetStatus());
                var err = AppAccessInfo.AddAccessRecord(AppInfoProvider.DataPath);
                if (err != null)
                    logger.Error(err, "Failed to add access record");

                PkgLoader.InitDefaults();
                BinaryStorageManagerFactory.Init((folder, readOnly) => new LmdbManager(folder, readOnly));
                Controls.Chart.Customizer.Init();
            }
        }


        private static void ResolveAppInfo()
        {
            AppInfoProvider.Init();

            if (Execute.InDesignMode)
                return;

            if (AppInfoProvider.HasError)
            {
                MessageBox.Show($"Failed to resolve app folder. Check windows logs for details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.FailFast("Failed to resolve app folder", AppInfoProvider.Error);
            }
            else if (string.IsNullOrEmpty(AppInfoProvider.DataPath))
            {
                const string err = "Unexpected error: app folder resolved to empty string";
                MessageBox.Show(err, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.FailFast(err);
            }
            Directory.SetCurrentDirectory(AppInfoProvider.DataPath);
        }

        private static void ConfigureCaliburn()
        {
            ViewLocator.AddDefaultTypeMapping("Page");
            ViewLocator.AddDefaultTypeMapping("Dialog");

            ViewLocator.LocateForModelType = (modelType, displayLocation, context) =>
            {
                var viewType = ViewLocator.LocateTypeForModelType(modelType, displayLocation, context);

                if (viewType == null)
                    return AutoViewLocator.CreateView(modelType, context);

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

            ActorSharp.Actor.UnhandledException += (e) =>
            {
                logger.Error(e, "Unhandled Exception on Actor level!");
            };

            ActorSystem.ActorErrors.Subscribe(ex => logger.Error(ex));
            ActorSystem.ActorFailed.Subscribe(ex => logger.Fatal(ex));
        }

        private void ConfigurateLogger()
        {
            var config = new LoggingConfiguration();

            var p = new NLogFileParams { LogDirectory = EnvService.Instance.JournalFolder, Layout = NLogHelper.SimpleLogLayout };

            p.FileNameSuffix = "journal";
            p.ArchiveDirectory = Path.Combine(p.LogDirectory, "Archive");

            var journalTarget = NLogHelper.CreateAsyncFileTarget(p, 500, 10000);
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, journalTarget, $"*Journal*", true);

            p.LogDirectory = EnvService.Instance.LogFolder;
            p.ArchiveDirectory = Path.Combine(p.LogDirectory, "Archive");

            p.FileNameSuffix = "alert";
            var alertTarget = NLogHelper.CreateAsyncFileTarget(p, 100, 1000);
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, alertTarget, $"*{nameof(AlertViewModel)}", true);

            p.Layout = NLogHelper.NormalLogLayout;

            p.FileNameSuffix = "terminal";
            var logTarget = NLogHelper.CreateAsyncFileTarget(p, 500, 10000);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logTarget);

            p.FileNameSuffix = "terminal-error";
            var errTarget = NLogHelper.CreateAsyncFileTarget(p, 200, 1000);
            config.AddRule(LogLevel.Error, LogLevel.Fatal, errTarget);

            NLog.LogManager.Configuration = config;

            NonBlockingFileCompressor.Setup();
            AlgoLoggerFactory.Init(NLogLoggerAdapter.Create);
            HealthChecker.Start();
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
                App.Current.Shutdown();
                return;
            }

            if (!_instanceRestrictor.EnsureSingleInstace())
                App.Current.Shutdown();
            else
            {
                CertificateProvider.InitServer(SslImport.LoadServerCertificate(), SslImport.LoadServerPrivateKey());

                var enableLogs = Properties.Settings.Default.EnableConnectionLogs;
                var connectionOptions = ConnectionOptions.CreateForTerminal(enableLogs, EnvService.Instance.LogFolder, AppVersionInfo.Current.Version);

                var accountSettings = new AccountModelSettings("0")
                {
                    ConnectionSettings = new ConnectionSettings
                    {
                        AccountFactory = (options, loggerId) => new SfxInterop(options, loggerId),
                        Options = connectionOptions,
                    },
                };

                var clientHandler = new ClientModel.ControlHandler(accountSettings);
                var dataHandler = clientHandler.CreateDataHandler();
                await dataHandler.Init();

                _container.RegisterInstance(typeof(ClientModel.Data), null, dataHandler);
                _container.Singleton<IWindowManager, Caliburn.Micro.WindowManager>();
                _container.Singleton<IEventAggregator, EventAggregator>();
                _container.Singleton<ShellViewModel>();

                _shell = _container.GetInstance<ShellViewModel>();
                _shell.Deactivated += Shell_Deactivated; ;

                await DisplayRootViewForAsync<ShellViewModel>();
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


        private static bool HasWriteAccess()
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
                        MessageBox.Show($"Don't have access to write in directory {EnvService.Instance.AppFolder}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
