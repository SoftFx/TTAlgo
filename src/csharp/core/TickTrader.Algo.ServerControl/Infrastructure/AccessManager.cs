using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.Algo.ServerControl
{
    public class AccessManager : IAccessManager
    {
        public ClientClaims.Types.AccessLevel Level { get; }

        public bool HasViewerAccess { get; }

        public bool HasDealerAccess { get; }

        public bool HasAdminAccess { get; }


        public AccessManager(ClientClaims.Types.AccessLevel level)
        {
            Level = level;

            HasViewerAccess = Level == ClientClaims.Types.AccessLevel.Viewer || Level == ClientClaims.Types.AccessLevel.Dealer || Level == ClientClaims.Types.AccessLevel.Admin;
            HasDealerAccess = Level == ClientClaims.Types.AccessLevel.Dealer || Level == ClientClaims.Types.AccessLevel.Admin;
            HasAdminAccess = Level == ClientClaims.Types.AccessLevel.Admin;
        }


        public bool CanGetSnapshot() => HasViewerAccess;

        public bool CanSubscribeToUpdates() => HasViewerAccess;

        public bool CanGetApiMetadata() => HasViewerAccess;

        public bool CanGetMappingsInfo() => HasViewerAccess;

        public bool CanGetSetupContext() => HasViewerAccess;

        public bool CanGetAccountMetadata() => HasViewerAccess;

        public bool CanGetBotList() => HasViewerAccess;

        public bool CanAddBot() => HasAdminAccess;

        public bool CanRemoveBot() => HasAdminAccess;

        public bool CanStartBot() => HasDealerAccess;

        public bool CanStopBot() => HasDealerAccess;

        public bool CanChangeBotConfig() => HasAdminAccess;

        public bool CanGetAccountList() => HasViewerAccess;

        public bool CanAddAccount() => HasAdminAccess;

        public bool CanRemoveAccount() => HasAdminAccess;

        public bool CanChangeAccount() => HasAdminAccess;

        public bool CanTestAccount() => HasDealerAccess;

        public bool CanTestAccountCreds() => HasDealerAccess;

        public bool CanGetPackageList() => HasViewerAccess;

        public bool CanUploadPackage() => HasAdminAccess;

        public bool CanRemovePackage() => HasAdminAccess;

        public bool CanDownloadPackage() => HasAdminAccess;

        public bool CanGetBotStatus() => HasViewerAccess;

        public bool CanGetBotLogs() => HasViewerAccess;

        public bool CanGetAlerts() => HasViewerAccess;

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

        public bool CanClearBotFolder() => HasAdminAccess;

        public bool CanDeleteBotFile() => HasAdminAccess;

        public bool CanUploadBotFile() => HasAdminAccess;

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
