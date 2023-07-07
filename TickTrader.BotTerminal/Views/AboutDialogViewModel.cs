using Caliburn.Micro;
using TickTrader.Algo.AppCommon;

namespace TickTrader.BotTerminal
{
    class AboutDialogViewModel : Screen
    {
        public string Version { get; private set; }

        public AboutDialogViewModel()
        {
            DisplayName = $"About - {EnvService.Instance.ApplicationName}";
            var appVersion = AppVersionInfo.Current;
            Version = $"Version: {appVersion.Version} ({appVersion.BuildDate})";

//#if DEBUG
//            for (var i = 0; i < 5; i++)
//                GC.Collect(2, GCCollectionMode.Forced, true);
//#endif
        }


        public void Ok()
        {
            TryCloseAsync();
        }
    }
}
