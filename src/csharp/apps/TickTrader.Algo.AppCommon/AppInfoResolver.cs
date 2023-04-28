using System;
using System.IO;

namespace TickTrader.Algo.AppCommon
{
    public static class AppInfoResolver
    {
        public const string DataFileName = "appinfo.json";

        private static readonly object _staticLock = new();

        private static bool _isInit, _isDevDir;
        private static AppInfo _info;
        private static string _appInfoFolder, _dataPath;


        public static AppInfo Info
        {
            get
            {
                if (!_isInit)
                    Init();

                return _info;
            }
        }

        public static string DataPath
        {
            get
            {
                if (!_isInit)
                    Init();

                return _dataPath;
            }
        }

        public static string AppInfoFolder
        {
            get
            {
                if (!_isInit)
                    Init();

                return _appInfoFolder;
            }
        }

        public static bool IsDevDir
        {
            get
            {
                if (!_isInit)
                    Init();

                return _isDevDir;
            }
        }

        public static Exception Error { get; private set; }

        public static bool HasError => Error != null;

        public static bool IgnorePortableFlag { get; set; }

        public static string AppShortName { get; set; } = "Terminal";


        public static void Init()
        {
            lock (_staticLock)
            {
                if (_isInit)
                    return;

                try
                {
                    ResolveAppInfoFolder();
                    var appInfoPath = Path.Combine(_appInfoFolder, DataFileName);

                    if (_isDevDir)
                        InitDevDir(appInfoPath);

                    if (!File.Exists(appInfoPath))
                        throw new Exception($"Invalid install: Can't find '{DataFileName}' in '{_appInfoFolder}'");

                    _info = AppInfo.LoadFromJson(appInfoPath);
                    _dataPath = (IgnorePortableFlag || _info.IsPortable)
                        ? ResolvePortablePath(_info)
                        : ResolveLocalAppDataPath(_info);
                }
                catch (Exception ex)
                {
                    Error = ex;
                    _dataPath = null;
                }

                _isInit = true;
            }
        }


        private static void ResolveAppInfoFolder()
        {
            var binFolder = AppDomain.CurrentDomain.BaseDirectory;
            var pathParts = binFolder.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            var n = pathParts.Length;

            // Expected install dir pattern: {some_path}/app/bin/current
            if (n <= 3)
                throw new Exception("Invalid install path. Can't find app directory");

            // Expected build dir pattern: {some_path}/project/bin/[debug|release]/{tfm}
            _isDevDir = n > 4
                && string.Equals(pathParts[n - 3], "bin", StringComparison.OrdinalIgnoreCase)
                && (string.Equals(pathParts[n - 2], "release", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(pathParts[n - 2], "debug", StringComparison.OrdinalIgnoreCase));

            _appInfoFolder = Path.GetFullPath(Path.Combine(binFolder, "..", "..")); // simplify path
        }

        private static void InitDevDir(string appInfoPath)
        {
            if (File.Exists(appInfoPath))
                return;

            var appInfo = new AppInfo { IsPortable = true, InstallId = "dev", DataPath = "data" };
            appInfo.SaveAsJson(appInfoPath);
        }

        private static string ResolveLocalAppDataPath(AppInfo info)
        {
            var installId = info.InstallId;
            if (string.IsNullOrEmpty(installId))
                throw new Exception($"Invalid install: InstallId = '{installId}'");

            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(localAppData, "TickTrader Algo", $"{AppShortName} {installId}");
        }

        private static string ResolvePortablePath(AppInfo info)
        {
            var dataPath = Environment.ExpandEnvironmentVariables(info.DataPath);
            return string.IsNullOrEmpty(dataPath)
                ? _appInfoFolder
                : Path.IsPathRooted(dataPath)
                    ? dataPath
                    : Path.Combine(_appInfoFolder, dataPath);
        }
    }
}
