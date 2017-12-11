namespace TickTrader.Algo.Protocol
{
    public class VersionSpec
    {
        // Should be syncronized with BotAgent.net version
        public const int MajorVersion = 1;
        public const int MinorVersion = 0;


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


        internal static int ResolveVersion(int clientMajorVersion, int clientMinorVersion, out string reason)
        {
            reason = "";
            if (MajorVersion != clientMajorVersion)
            {
                reason = $"Major version mismatch: server - {MajorVersion}, client - {clientMajorVersion}";
                return -1;
            }
            if (MinorVersion > clientMinorVersion)
            {
                reason = "Server doesn't support older clients";
                return -1;
            }
            return MinorVersion;
        }


        public bool SupportsBlackjack => CurrentVersion == MinorVersion;
    }
}
