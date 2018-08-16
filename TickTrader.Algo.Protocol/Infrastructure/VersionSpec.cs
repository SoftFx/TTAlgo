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
