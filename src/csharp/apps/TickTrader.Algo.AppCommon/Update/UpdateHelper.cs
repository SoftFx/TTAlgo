namespace TickTrader.Algo.AppCommon.Update
{
    public static class UpdateHelper
    {
        public const string UpdaterFileName = "TickTrader.Algo.Updater.exe";
        public const string TerminalFileName = "TickTrader.AlgoTerminal.exe";
        public const string ServerFileName = "TickTrader.AlgoServer.exe";


        public static string GetAppExeFileName(UpdateAppTypes appType)
        {
            return appType switch
            {
                UpdateAppTypes.Terminal => "TickTrader.AlgoTerminal.exe",
                UpdateAppTypes.Server => "TickTrader.AlgoServer.exe",
                _ => string.Empty
            };
        }
    }
}
