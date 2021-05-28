using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Lib;

namespace TickTrader.BotTerminal
{
    class AboutDialogViewModel : Screen
    {
        private static readonly string _version;
        private static readonly string _buildDate;


        public string Version { get; private set; }


        static AboutDialogViewModel()
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            _version = entryAssembly.GetName().Version.ToString();
            _buildDate = entryAssembly.GetLinkerTime().ToString("yyyy.MM.dd");
        }

        public AboutDialogViewModel()
        {
            DisplayName = $"About - {EnvService.Instance.ApplicationName}";
            Version = $"Version: {_version} ({_buildDate})";
        }


        public void Ok()
        {
            TryCloseAsync();
        }
    }
}
