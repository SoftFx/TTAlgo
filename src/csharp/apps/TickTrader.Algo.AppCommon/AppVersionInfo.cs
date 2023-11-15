using System;
using System.Diagnostics;
using System.Reflection;

namespace TickTrader.Algo.AppCommon
{
    public sealed class AppVersionInfo : IComparable<AppVersionInfo>, IEquatable<AppVersionInfo>
    {
        public const string BuildDateFormat = "yyyy.MM.dd";

        private static AppVersionInfo _current;


        public static AppVersionInfo Current
        {
            get
            {
                if (_current is null)
                {
                    try
                    {
                        _current = GetFromDllPath(Assembly.GetEntryAssembly().Location);
                    }
                    catch (Exception)
                    {
                        _current = new();
                    }
                }

                return _current;
            }
        }


        public string Version { get; }

        public string BuildDate { get; }


        public AppVersionInfo()
        {
            Version = "0.0.0";
            BuildDate = "1970.01.01";
        }

        public AppVersionInfo(string version, string buildDate)
        {
            Version = version;
            BuildDate = buildDate;
        }


        public override string ToString()
        {
            return $"{Version} ({BuildDate})";
        }

        public override bool Equals(object other)
        {
            if (other is AppVersionInfo appVersion)
                return Equals(appVersion);

            return false;
        }

        public override int GetHashCode() => HashCode.Combine(Version, BuildDate);

        public int CompareTo(AppVersionInfo other)
        {
            var versionCmp = CompareVersions(Version, other.Version);
            return versionCmp == 0
                ? CompareBuildDates(BuildDate, other.BuildDate)
                : versionCmp;
        }

        public bool Equals(AppVersionInfo other) => CompareTo(other) == 0;


        public static AppVersionInfo GetFromDllPath(string dllPath)
        {
            var version = FileVersionInfo.GetVersionInfo(dllPath).FileVersion;
            var releaseDate = Core.Lib.AssemblyExtensions.GetLinkerTime(dllPath).ToString(BuildDateFormat);
            return new AppVersionInfo(version, releaseDate);
        }

        public static int CompareVersions(string v1, string v2)
        {
            if (v1 == null && v2 == null)
                return 0;
            if (v1 == null)
                return -1;
            if (v2 == null)
                return 1;

            var v1Parts = v1.Split(".");
            var v2Parts = v2.Split(".");

            for (var i = 0; i < Math.Max(v1Parts.Length, v2Parts.Length); i++)
            {
                // -2 - no component on i-th place
                // -1 - bad component on i-th place
                int num1 = -2, num2 = -2;

                if (i < v1Parts.Length && !int.TryParse(v1Parts[i], out num1))
                    num1 = -1;

                if (i < v2Parts.Length && !int.TryParse(v2Parts[i], out num2))
                    num2 = -1;

                var cmp = num1.CompareTo(num2);
                if (cmp != 0)
                    return cmp;
            }

            return 0;
        }

        public static int CompareBuildDates(string d1, string d2)
            // Fixed date format expected.
            => string.CompareOrdinal(d1, d2);

        public static bool operator ==(AppVersionInfo a, AppVersionInfo b) => a.Equals(b);
        public static bool operator !=(AppVersionInfo a, AppVersionInfo b) => !a.Equals(b);

        public static bool operator >(AppVersionInfo a, AppVersionInfo b) => a.CompareTo(b) > 0;
        public static bool operator <(AppVersionInfo a, AppVersionInfo b) => a.CompareTo(b) < 0;

        public static bool operator >=(AppVersionInfo a, AppVersionInfo b) => a.CompareTo(b) >= 0;
        public static bool operator <=(AppVersionInfo a, AppVersionInfo b) => a.CompareTo(b) <= 0;
    }
}
