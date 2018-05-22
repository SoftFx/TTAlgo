using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol.Sfx
{
    public class VersionSpec
    {
        public static int MajorVersion => Info.BotAgent.MajorVersion;

        public static int MinorVersion => Info.BotAgent.MinorVersion;

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

        public bool SupportsNewProtocol => CurrentVersion >= 2;

        public bool SupportsConnectionState => CurrentVersion >= 2;
    }
}
