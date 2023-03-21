using System;
using System.IO;

namespace TickTrader.Algo.AppCommon
{
    public static class AppInfoResolver
    {
        public const string DataFileName = "appinfo.json";

        private static readonly object _staticLock = new();

        private static bool _isInit;
        private static AppInfo _info;
        private static string _appFolder, _dataPath;


        public static AppInfo Info
        {
            get
            {
                if (!_isInit)
                    Init();

                return _info;
            }
        }

        public static string AppFolder
        {
            get
            {
                if (!_isInit)
                    Init();

                return _appFolder;
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

        public static Exception Error { get; private set; }

        public static bool HasError => Error != null;


        public static void Init()
        {
            lock (_staticLock)
            {
                if (_isInit)
                    return;

                _isInit = true;
                try
                {
                    _appFolder = GetAppFolder(out var isDevDir);
                    var appInfoPath = Path.Combine(_appFolder, DataFileName);

                    if (isDevDir)
                        InitDevDir(appInfoPath);

                    if (!File.Exists(appInfoPath))
                        throw new Exception($"Invalid install: Can't find '{DataFileName}'");

                    _info = AppInfo.LoadFromJson(appInfoPath);
                    _dataPath = _info.IsPortable
                        ? ResolvePortablePath(_info, _appFolder)
                        : ResolveLocalAppDataPath(_info);
                }
                catch (Exception ex)
                {
                    Error = ex;
                    _dataPath = null;
                }
            }
        }


        private static string GetAppFolder(out bool isDevDir)
        {
            var binFolder = AppDomain.CurrentDomain.BaseDirectory;
            var pathParts = binFolder.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            var n = pathParts.Length;

            // Expected install dir pattern: {some_path}/app/bin/current
            if (n <= 3)
                throw new Exception("Invalid install path. Can't find app directory");

            // Expected build dir pattern: {some_path}/project/bin/[debug|release]/{tfm}
            isDevDir = n > 4
                && string.Equals(pathParts[n - 3], "bin", StringComparison.OrdinalIgnoreCase)
                && (string.Equals(pathParts[n - 2], "release", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(pathParts[n - 2], "debug", StringComparison.OrdinalIgnoreCase));

            return Path.GetFullPath(Path.Combine(binFolder, "..", "..")); // simplify path
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
            return Path.Combine(localAppData, "TickTrader Algo", $"Terminal {installId}");
        }

        private static string ResolvePortablePath(AppInfo info, string appFolder)
        {
            var dataPath = info.DataPath;
            return string.IsNullOrEmpty(dataPath)
                ? _appFolder
                : Path.IsPathRooted(dataPath)
                    ? dataPath
                    : Path.Combine(_appFolder, dataPath);
        }
    }
}
