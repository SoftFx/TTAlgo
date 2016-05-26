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
            DLinq.Test();
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
            var logTarget = new FileTarget()
            {
                FileName = Path.Combine(EnvService.Instance.LogFolder, "Log-${shortdate}.txt"),
                ArchiveFileName = Path.Combine(Path.Combine(EnvService.Instance.LogFolder, "Archives"), "Log-${shortdate}.txt"),
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveNumbering = ArchiveNumberingMode.Date,
                ArchiveDateFormat = "yyyy-MM-dd",
                MaxArchiveFiles = 30
            };

            var journalTarget = new FileTarget()
            {
                FileName = Path.Combine(EnvService.Instance.JournalFolder, "Journal-${shortdate}.txt"),
                Layout = "${longdate} | ${message}"
            };

            var rule1 = new LoggingRule(string.Concat("*",nameof(EventJournal)), LogLevel.Trace, journalTarget) { Final = true };
            var rule2 = new LoggingRule("*", LogLevel.Debug, logTarget);

            var config = new LoggingConfiguration();
            config.AddTarget(nameof(logTarget), logTarget);
            config.AddTarget(nameof(journalTarget), journalTarget);
            config.LoggingRules.Add(rule1);
            config.LoggingRules.Add(rule2);

            NLog.LogManager.Configuration = config;
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
