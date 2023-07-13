namespace TickTrader.Algo.Domain.ServerControl
{
    public partial class ServerVersionRequest
    {
        public static ServerVersionRequest Instance { get; } = new ServerVersionRequest();
    }

    public partial class ServerVersionInfo
    {
        public ServerVersionInfo(string version, string releaseDate)
        {
            Version = version;
            ReleaseDate = releaseDate;
        }
    }

    public partial class StartServerUpdateRequest
    {
        public StartServerUpdateRequest(string releaseId)
        {
            ReleaseId = releaseId;
        }

        public StartServerUpdateRequest(string localPath, bool ownsFile)
        {
            LocalPath = localPath;
            OwnsLocalFile = ownsFile;
        }
    }

    public partial class StartServerUpdateResponse
    {
        public StartServerUpdateResponse(UpdateServiceStatusInfo status)
        {
            Status = status;
        }
    }

    public partial class ServerUpdateListRequest
    {
        private static readonly ServerUpdateListRequest _cached = new ServerUpdateListRequest { Forced = false };
        private static readonly ServerUpdateListRequest _forced = new ServerUpdateListRequest { Forced = true };


        public static ServerUpdateListRequest Get(bool forced) => forced ? _forced : _cached;
    }

    public partial class UpdateServiceStatusInfo
    {
        public UpdateServiceStatusInfo(AutoUpdateEnums.Types.ServiceStatus status, string errorDetails, string targetVersion)
        {
            Status = status;
            ErrorDetails = errorDetails;
            TargetVersion = targetVersion;
        }
    }
}
