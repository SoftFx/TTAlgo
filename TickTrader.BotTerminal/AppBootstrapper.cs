using Caliburn.Micro;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.IO;
using System.Windows;

namespace TickTrader.BotTerminal
{
    public class AppBootstrapper : BootstrapperBase
    {
        private static readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly AutoViewManager autoViewLocator = new AutoViewManager();

        private AppInstanceRestrictor _instanceRestrictor = new AppInstanceRestrictor();

        public AppBootstrapper()
        {
            LocaleSelector.Instance.ActivateDefault();

            Initialize();

            ConfigureCaliburn();
            ConfigurateLogger();
            ConfigureGlobalWpfExceptionHandling();
        }

        public static AutoViewManager AutoViewLocator => autoViewLocator;

        private void ConfigureCaliburn()
        {
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
        }

        private void ConfigureGlobalWpfExceptionHandling()
        {
            Application.DispatcherUnhandledException += (s, e) =>
            {
                e.Handled = true;
                logger.Error(e.Exception, "Unhandled Exception on Dispatcher level! Note to QA: this is definitly a bug!");
            };
        }

        private void ConfigurateLogger()
        {
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

        protected override void OnExit(object sender, EventArgs e)
        {
            base.OnExit(sender, e);

            _instanceRestrictor.Dispose();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            if (!_instanceRestrictor.EnsureSingleInstace())
                Application.Current.Shutdown();
            else
                DisplayRootViewFor<ShellViewModel>();
        }
    }
}
