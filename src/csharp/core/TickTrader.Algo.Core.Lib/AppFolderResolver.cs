using System;
using System.IO;

namespace TickTrader.Algo.Core.Lib
{
    public static class AppFolderResolver
    {
        public const string AppFolderSwitchName = "TickTrader.Algo.AppFolder";

        private static string _appFolder;
        private static bool _isInit;


        public static string Result
        {
            get
            {
                // Resolve once and keep cached
                if (!_isInit)
                {
                    _isInit = true;
                    _appFolder = Resolve();
                }

                return _appFolder;
            }
        }

        public static Exception Error { get; private set; }

        public static bool HasError => Error != null;


        private static string Resolve()
        {
            try
            {
                if (!TryGetConfiguredPath(out var appFolder))
                    appFolder = GetFallbackPath();

                return Environment.ExpandEnvironmentVariables(appFolder);
            }
            catch (Exception ex)
            {
                Error = ex;
                _appFolder = null;
            }

            return null;
        }

        private static string GetFallbackPath()
        {
            var binFolder = AppDomain.CurrentDomain.BaseDirectory;
            var pathParts = binFolder.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            var n = pathParts.Length;

            // Expected install dir pattern: {some_path}/app/bin/current
            if (n <= 3)
                throw new Exception("Invalid install path. Can't initialize fallback directory");

            // Expected build dir pattern: {some_path}/project/bin/[debug|release]/{tfm}
            var isBuildDir = n > 4
                && string.Equals(pathParts[n - 3], "bin", StringComparison.OrdinalIgnoreCase)
                && (string.Equals(pathParts[n - 2], "release", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(pathParts[n - 2], "debug", StringComparison.OrdinalIgnoreCase));

            var appFolder = isBuildDir
                ? Path.Combine(binFolder, "..", "..", "data")
                : Path.Combine(binFolder, "..", "..");
            appFolder = Path.GetFullPath(appFolder); // simplify path

            return appFolder;
        }

        private static bool TryGetConfiguredPath(out string path)
        {
            path = AppContext.GetData(AppFolderSwitchName) as string;
            return !string.IsNullOrEmpty(path);
        }
    }
}
