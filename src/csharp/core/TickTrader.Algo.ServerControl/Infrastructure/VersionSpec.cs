﻿namespace TickTrader.Algo.ServerControl
{
    public class VersionSpec
    {
        public static int MajorVersion => 2;

        public static int MinorVersion => 0;

        public static string LatestVersion => $"{MajorVersion}.{MinorVersion}";


        public int CurrentVersion { get; }

        public string CurrentVersionStr => $"{MajorVersion}.{CurrentVersion}";

        public bool SupportAlerts => CurrentVersion > 0;

        public bool SupportMainToken => CurrentVersion > 0;

        public bool SupportModelTimeframe => CurrentVersion > 1;

        internal VersionSpec()
        {
            CurrentVersion = MinorVersion;
        }

        internal VersionSpec(int currentVersion)
        {
            CurrentVersion = currentVersion;
        }


        internal static bool CheckClientCompatibility(int clientMajorVersion, int clientMinorVersion, out string error)
        {
            error = "";

            if (MajorVersion != clientMajorVersion)
            {
                error = $"Major version mismatch: server - {MajorVersion}, client - {clientMajorVersion}";
                return false;
            }
            if (MinorVersion > clientMinorVersion)
            {
                error = "Server doesn't support older clients";
                return false;
            }
            return true;
        }


        public bool SupportsBlackjack => CurrentVersion == MinorVersion;
    }
}