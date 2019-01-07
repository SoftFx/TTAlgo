using ActorSharp;
using Caliburn.Micro;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TickTrader.Algo.Common.Model;

namespace TickTrader.BotTerminal
{
    public class AppBootstrapper : BootstrapperBase
    {
        private static readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly AutoViewManager autoViewLocator = new AutoViewManager();

        private AppInstanceRestrictor _instanceRestrictor = new AppInstanceRestrictor();
        private SimpleContainer _container = new SimpleContainer();
        private ShellViewModel _shell;

        public AppBootstrapper()
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            LocaleSelector.Instance.ActivateDefault();

            Initialize();

            ConfigureCaliburn();
            ConfigurateLogger();
            ConfigureGlobalExceptionHandling();
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
            NonBlockingFileCompressor.Setup();

            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition("botName", typeof(BotNameLayoutRenderer));

            var debuggerTarget = new DebuggerTarget() { Layout = "${logger} -> ${message} ${exception:format=tostring}" };

            var logTarget = new FileTarget()
            {
                Layout = "${longdate} | ${logger} -> ${message} ${exception:format=tostring}",
                FileName = Path.Combine(EnvService.Instance.LogFolder, "terminal.log"),
                ArchiveFileName = Path.Combine(Path.Combine(EnvService.Instance.LogFolder, "Archives"), "terminal-{#}.zip"),
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveNumbering = ArchiveNumberingMode.Date,
                EnableArchiveFileCompression = true,
            };

            var journalTarget = new FileTarget()
            {
                FileName = Path.Combine(EnvService.Instance.JournalFolder, "Journal-${shortdate}.txt"),
                Layout = "${longdate} | ${message}"
            };

            var botInfoTarget = new FileTarget()
            {
                FileName = Path.Combine(EnvService.Instance.BotLogFolder, "${botName}/Log.txt"),
                Layout = "${longdate} | ${message}",
                ArchiveFileName = Path.Combine(Path.Combine(Path.Combine(EnvService.Instance.BotLogFolder, "${botName}"), "Archives"), "Log-{#}.zip"),
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveNumbering = ArchiveNumberingMode.Date,
                EnableArchiveFileCompression = true
            };

            var botErrorTarget = new FileTarget()
            {
                FileName = Path.Combine(EnvService.Instance.BotLogFolder, "${botName}/Error.txt"),
                Layout = "${longdate} | ${message}",
                ArchiveFileName = Path.Combine(Path.Combine(Path.Combine(EnvService.Instance.BotLogFolder, "${botName}"), "Archives"), "Error-{#}.zip"),
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveNumbering = ArchiveNumberingMode.Date,
                EnableArchiveFileCompression = true
            };

            var botStatusTarget = new FileTarget()
            {
                FileName = Path.Combine(EnvService.Instance.BotLogFolder, "${botName}/Status.txt"),
                Layout = "${longdate} | ${message}",
                ArchiveFileName = Path.Combine(Path.Combine(Path.Combine(EnvService.Instance.BotLogFolder, "${botName}"), "Archives"), "Status-{#}.zip"),
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveNumbering = ArchiveNumberingMode.Date,
                EnableArchiveFileCompression = true
            };

            var ruleForJournalTarget = new LoggingRule(string.Concat("*", nameof(EventJournal)), LogLevel.Trace, journalTarget) { Final = true };
            var ruleForBotInfoTarget = new LoggingRule(string.Concat(LoggerHelper.LoggerNamePrefix, "*"), LogLevel.Debug, botInfoTarget) { Final = true };
            var ruleForBotErrorTarget = new LoggingRule(string.Concat(LoggerHelper.LoggerNamePrefix, "*"), LogLevel.Error, botErrorTarget);
            var ruleForBotStatusTarget = new LoggingRule(string.Concat(LoggerHelper.LoggerNamePrefix, "*"), LogLevel.Trace, LogLevel.Trace, botStatusTarget) { Final = true };
            var ruleForLogTarget = new LoggingRule();
            ruleForLogTarget.LoggerNamePattern = "*";
            ruleForLogTarget.EnableLoggingForLevels(LogLevel.Debug, LogLevel.Fatal);
            ruleForLogTarget.Targets.Add(debuggerTarget);
            ruleForLogTarget.Targets.Add(logTarget);

            var config = new LoggingConfiguration();

            config.LoggingRules.Add(ruleForJournalTarget);
            config.LoggingRules.Add(ruleForBotStatusTarget);
            config.LoggingRules.Add(ruleForBotErrorTarget);
            config.LoggingRules.Add(ruleForBotInfoTarget);
            config.LoggingRules.Add(ruleForLogTarget);

            NLog.LogManager.Configuration = config;

            Algo.Core.CoreLoggerFactory.Init(s => new AlgoLogAdapter(s));
        }

        //protected override void OnExit(object sender, EventArgs e)
        //{
        //    base.OnExit(sender, e);


        //}

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
            if (!_instanceRestrictor.EnsureSingleInstace())
                Application.Current.Shutdown();
            else
            {
                var connectionOptions = new ConnectionOptions()
                {
                    AutoReconnect = true,
                    EnableLogs = BotTerminal.Properties.Settings.Default.EnableConnectionLogs,
                    LogsFolder = EnvService.Instance.LogFolder
                };

                var clientHandler = new ClientModel.ControlHandler(connectionOptions, EnvService.Instance.FeedHistoryCacheFolder, FeedHistoryFolderOptions.ServerHierarchy);
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
                _shell.Deactivated += Shell_Deactivated;

                DisplayRootViewFor<ShellViewModel>();
            }
        }

        private async void Shell_Deactivated(object sender, DeactivationEventArgs e)
        {
            if (e.WasClosed)
            {
                await _shell.Shutdown();
                _instanceRestrictor.Dispose();
                App.Current.Shutdown();
            }
        }
    }
}
