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
        public ServerUpdateStatusResponse(AutoUpdate.Types.ServiceStatus status)
        {
            Status = status;
        }
    }

    public partial class StartServerUpdateRequest
    {
        public StartServerUpdateRequest(string downloadUrl)
        {
            DownloadUrl = downloadUrl;
        }
    }

    public partial class StartServerUpdateResponse
    {
        public StartServerUpdateResponse(AutoUpdate.Types.ServiceStatus status)
        {
            Status = status;
        }
    }

    public partial class ServerUpdateListRequest
    {
        public static ServerUpdateListRequest Instance { get; } = new ServerUpdateListRequest();
    }
}
