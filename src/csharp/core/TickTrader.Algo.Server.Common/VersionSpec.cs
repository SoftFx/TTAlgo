namespace TickTrader.Algo.Server.Common
{
    public class VersionSpec
    {
        public static int MajorVersion => 2;

        public static int MinorVersion => 2;

        public static string LatestVersion => $"{MajorVersion}.{MinorVersion}";


        public int CurrentVersion { get; }

        public string CurrentVersionStr => $"{MajorVersion}.{CurrentVersion}";


        public VersionSpec()
        {
            CurrentVersion = MinorVersion;
        }

        public VersionSpec(int currentVersion)
        {
            CurrentVersion = currentVersion;
        }


        public static bool CheckClientCompatibility(int clientMajorVersion, int clientMinorVersion, out string error)
        {
            error = string.Empty;

            if (MajorVersion != clientMajorVersion)
            {
                error = $"Major version mismatch: server - {MajorVersion}, client - {clientMajorVersion}";
                return false;
            }

            //if (MinorVersion > clientMinorVersion)
            //{
            //    error = "Server doesn't support older clients";
            //    return false;
            //}

            return true;
        }

        public static bool ClientSupports2FA(int clientVersion) => clientVersion >= 1;


        public bool SupportsBlackjack => CurrentVersion == MinorVersion;

        public bool SupportsAutoUpdate => CurrentVersion >= 2;
    }
}
