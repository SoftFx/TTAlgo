using Caliburn.Micro;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TickTrader.BotTerminal
{
    public class AppBootstrapper : BootstrapperBase
    {
        public AppBootstrapper()
        {
#if DEBUG
            Machinarium.Qnil.UnitTests.Launch();
#endif
            Initialize();

            ConfigurateLogger();

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

        private void ConfigurateLogger()
        {
            var debuggerTarget = new DebuggerTarget() { Layout = "${logger} -> ${message} ${exception:format=tostring}" };

            var logTarget = new FileTarget()
            {
                Layout = "${logger} -> ${message} ${exception:format=tostring}",
                FileName = Path.Combine(EnvService.Instance.LogFolder, "Log.txt"),
                ArchiveFileName = Path.Combine(Path.Combine(EnvService.Instance.LogFolder, "Archives"), "Archive-{#}.txt"),
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveNumbering = ArchiveNumberingMode.Date,
                ArchiveOldFileOnStartup = true,
                MaxArchiveFiles = 30
            };

            var journalTarget = new FileTarget()
            {
                FileName = Path.Combine(EnvService.Instance.JournalFolder, "Journal-${shortdate}.txt"),
                Layout = "${longdate} | ${message}"
            };

            var ruleForJournalTarget = new LoggingRule(string.Concat("*",nameof(EventJournal)), LogLevel.Trace, journalTarget) { Final = true };
            var ruleForLogTarget = new LoggingRule("*", LogLevel.Debug, logTarget);
            var ruleForDebuggerTarget = new LoggingRule("*", LogLevel.Debug, debuggerTarget);

            var config = new LoggingConfiguration();
            config.AddTarget(nameof(logTarget), logTarget);
            config.AddTarget(nameof(journalTarget), journalTarget);
            config.AddTarget(nameof(debuggerTarget), debuggerTarget);
            config.LoggingRules.Add(ruleForJournalTarget);
            config.LoggingRules.Add(ruleForLogTarget);
            config.LoggingRules.Add(ruleForDebuggerTarget);

            NLog.LogManager.Configuration = config;
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
