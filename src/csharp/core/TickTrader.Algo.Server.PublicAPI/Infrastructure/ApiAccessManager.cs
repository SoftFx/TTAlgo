using TickTrader.Algo.Server.Common;

namespace TickTrader.Algo.Server.PublicAPI
{
    internal sealed class ApiAccessManager : AccessManager, IAccessManager
    {
        public new ClientClaims.Types.AccessLevel Level { get; }


        public ApiAccessManager(ClientClaims.Types.AccessLevel level) : base()
        {
            Level = level;

            HasViewerAccess = Level <= ClientClaims.Types.AccessLevel.Viewer;
            HasDealerAccess = Level <= ClientClaims.Types.AccessLevel.Dealer;
            HasAdminAccess = Level <= ClientClaims.Types.AccessLevel.Admin;
        }


        public bool CanGetBotFolderInfo(PluginFolderInfo.Types.PluginFolderId folderId)
        {
            switch (folderId)
            {
                case PluginFolderInfo.Types.PluginFolderId.BotLogs:
                    return HasViewerAccess;
                default:
                    return HasAdminAccess;
            }
        }

        public bool CanDownloadBotFile(PluginFolderInfo.Types.PluginFolderId folderId)
        {
            switch (folderId)
            {
                case PluginFolderInfo.Types.PluginFolderId.BotLogs:
                    return HasViewerAccess;
                default:
                    return HasAdminAccess;
            }
        }
    }
}
