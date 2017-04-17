using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    class AboutDialogViewModel : Screen
    {
        public string Version { get; private set; }


        public AboutDialogViewModel()
        {
            DisplayName = $"About - {EnvService.Instance.ApplicationName}";
            Version = $"Version: {Assembly.GetEntryAssembly().GetName().Version}";
        }


        public void Ok()
        {
            TryClose();
        }
    }
}
