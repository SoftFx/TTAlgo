namespace TickTrader.Algo.Domain.ServerControl
{
    public partial class ServerVersionRequest
    {
        public static ServerVersionRequest Instance { get; } = new ServerVersionRequest();
    }

    public partial class ServerUpdateStatusRequest
    {
        public static ServerUpdateStatusRequest Instance { get; } = new ServerUpdateStatusRequest();
    }

    public partial class ServerUpdateStatusResponse
    {
        public ServerUpdateStatusResponse(AutoUpdateEnums.Types.ServiceStatus status)
        {
            Status = status;
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
        public StartServerUpdateResponse(AutoUpdateEnums.Types.ServiceStatus status)
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
}
