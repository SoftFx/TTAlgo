namespace TickTrader.Algo.AppCommon.Update
{
    // Has to be sync with build.cake:
    // Class definition
    // MinVersion == "1.24.0.0"
    // Executable == "TickTrader.Algo.Updater.exe"
    public class UpdateInfo
    {
        public string ReleaseVersion { get; set; }

        public string ReleaseDate { get; set; }

        public string MinVersion { get; set; }

        public string Executable { get; set; }

        public string Changelog { get; set; }


        public AppVersionInfo GetAppVersion() => new(ReleaseVersion, MinVersion);
    }
}
