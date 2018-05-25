namespace TickTrader.Algo.Protocol
{
    public class VersionSpec
    {
        public static int MajorVersion => 1;

        public static int MinorVersion => 0;

        public static string LatestVersion => $"{MajorVersion}.{MinorVersion}";


        public int CurrentVersion { get; }

        public string CurrentVersionStr => $"{MajorVersion}.{CurrentVersion}";


        internal VersionSpec()
        {
            CurrentVersion = MinorVersion;
        }

        internal VersionSpec(int currentVersion)
        {
            CurrentVersion = currentVersion;
        }


        internal static string CheckClientCompatibility(int clientMajorVersion, int clientMinorVersion)
        {
            if (MajorVersion != clientMajorVersion)
            {
                return $"Major version mismatch: server - {MajorVersion}, client - {clientMajorVersion}";
            }
            if (MinorVersion > clientMinorVersion)
            {
                return "Server doesn't support older clients";
            }
            return null;
        }


        public bool SupportsBlackjack => CurrentVersion == MinorVersion;
    }
}
