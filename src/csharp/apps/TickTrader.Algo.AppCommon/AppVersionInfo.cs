using System;
using System.Diagnostics;
using System.Reflection;

namespace TickTrader.Algo.AppCommon
{
    public class AppVersionInfo
    {
        private static AppVersionInfo _current;


        public static AppVersionInfo Current
        {
            get
            {
                if (_current == null)
                {
                    try
                    {
                        _current = GetFromDllPath(Assembly.GetEntryAssembly().Location);
                    }
                    catch (Exception)
                    {
                        _current = new AppVersionInfo("1.0.0.0", "1970.01.01");
                    }
                }

                return _current;
            }
        }


        public string Version { get; }

        public string BuildDate { get; }


        public AppVersionInfo(string version, string buildDate)
        {
            Version = version;
            BuildDate = buildDate;
        }


        public static AppVersionInfo GetFromDllPath(string dllPath)
        {
            var version = FileVersionInfo.GetVersionInfo(dllPath).FileVersion;
            var releaseDate = Core.Lib.AssemblyExtensions.GetLinkerTime(dllPath).ToString("yyyy.MM.dd");
            return new AppVersionInfo(version, releaseDate);
        }
    }
}
