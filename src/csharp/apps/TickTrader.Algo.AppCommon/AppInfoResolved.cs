using System.IO;
using System;

namespace TickTrader.Algo.AppCommon
{
    public class ResolveAppInfoRequest
    {
        public string BinFolder { get; init; } = AppDomain.CurrentDomain.BaseDirectory;

        public bool IgnorePortableFlag { get; init; }

        public string DataFileName { get; init; } = "appinfo.json";

        public string AppShortName { get; init; } = "Terminal";
    }

    public class AppInfoResolved
    {
        public ResolveAppInfoRequest Request { get; init; }

        public Exception Error { get; private set; }

        public bool HasError => Error != null;

        public AppInfo Info { get; private set; }

        public string DataPath { get; private set; }

        public string AppInfoFolder { get; private set; }

        public bool IsDevDir { get; private set; }


        private AppInfoResolved() { }


        public static AppInfoResolved Create(ResolveAppInfoRequest request)
        {
            var res = new AppInfoResolved { Request = request };
            try
            {
                res.ResolveAppInfoFolder();
                var appInfoPath = Path.Combine(res.AppInfoFolder, request.DataFileName);

                if (res.IsDevDir)
                    InitDevDir(appInfoPath);

                if (!File.Exists(appInfoPath))
                    throw new Exception($"Invalid install: Can't find '{request.DataFileName}' in '{res.AppInfoFolder}'");

                res.Info = AppInfo.LoadFromJson(appInfoPath);
                res.DataPath = (request.IgnorePortableFlag || res.Info.IsPortable)
                    ? res.ResolvePortablePath()
                    : res.ResolveLocalAppDataPath();
            }
            catch (Exception ex)
            {
                res.Error = ex;
                res.DataPath = null;
            }

            return res;
        }


        private void ResolveAppInfoFolder()
        {
            var binFolder = Request.BinFolder;
            var pathParts = binFolder.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            var n = pathParts.Length;

            // Expected install dir pattern: {some_path}/app/bin/current
            if (n <= 3)
                throw new Exception("Invalid install path. Can't find app directory");

            // Expected build dir pattern: {some_path}/project/bin/[debug|release]/{tfm}
            IsDevDir = n > 4
                && string.Equals(pathParts[n - 3], "bin", StringComparison.OrdinalIgnoreCase)
                && (string.Equals(pathParts[n - 2], "release", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(pathParts[n - 2], "debug", StringComparison.OrdinalIgnoreCase));

            AppInfoFolder = Path.GetFullPath(Path.Combine(binFolder, "..", "..")); // simplify path
        }

        private static void InitDevDir(string appInfoPath)
        {
            if (File.Exists(appInfoPath))
                return;

            var appInfo = new AppInfo { IsPortable = true, InstallId = "dev", DataPath = "data" };
            appInfo.SaveAsJson(appInfoPath);
        }

        private string ResolveLocalAppDataPath()
        {
            var installId = Info.InstallId;
            if (string.IsNullOrEmpty(installId))
                throw new Exception($"Invalid install: InstallId = '{installId}'");

            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(localAppData, "TickTrader Algo", $"{Request.AppShortName} {installId}");
        }

        private string ResolvePortablePath()
        {
            var dataPath = Environment.ExpandEnvironmentVariables(Info.DataPath);
            return string.IsNullOrEmpty(dataPath)
                ? AppInfoFolder
                : Path.IsPathRooted(dataPath)
                    ? dataPath
                    : Path.Combine(AppInfoFolder, dataPath);
        }
    }
}
