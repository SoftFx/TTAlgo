using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.Algo.Server.Common
{
    public class AccessManager
    {
        public ClientClaims.Types.AccessLevel Level { get; }

        public bool HasViewerAccess { get; protected set; }

        public bool HasDealerAccess { get; protected set; }

        public bool HasAdminAccess { get; protected set; }


        protected AccessManager() { }

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

        public bool CanAddPlugin() => HasAdminAccess;

        public bool CanRemovePlugin() => HasAdminAccess;

        public bool CanStartPlugin() => HasDealerAccess;

        public bool CanStopPlugin() => HasDealerAccess;

        public bool CanChangePluginConfig() => HasAdminAccess;

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

        public bool CanGetPluginStatus() => HasViewerAccess;

        public bool CanGetPluginLogs() => HasViewerAccess;

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
