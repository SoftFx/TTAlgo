using TickTrader.Algo.Common.Info;

namespace TickTrader.Algo.Protocol
{
    public enum AccessLevels
    {
        Viewer = 0,
        Dealer = 1,
        Admin = 2,
    }


    public class AccessManager
    {
        public AccessLevels Level { get; }

        public bool HasViewerAccess { get; }

        public bool HasDealerAccess { get; }

        public bool HasAdminAccess { get; }


        public AccessManager(AccessLevels level)
        {
            Level = level;

            HasViewerAccess = Level == AccessLevels.Viewer || Level == AccessLevels.Dealer || Level == AccessLevels.Admin;
            HasDealerAccess = Level == AccessLevels.Dealer || Level == AccessLevels.Admin;
            HasAdminAccess = Level == AccessLevels.Admin;
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

        public bool CanGetBotFolderInfo(BotFolderId folderId)
        {
            switch (folderId)
            {
                case BotFolderId.BotLogs:
                    return HasViewerAccess;
                default:
                    return HasAdminAccess;
            }
        }

        public bool CanClearBotFolder() => HasAdminAccess;

        public bool CanDeleteBotFile() => HasAdminAccess;

        public bool CanUploadBotFile() => HasAdminAccess;

        public bool CanDownloadBotFile(BotFolderId folderId)
        {
            switch (folderId)
            {
                case BotFolderId.BotLogs:
                    return HasViewerAccess;
                default:
                    return HasAdminAccess;
            }
        }
    }
}
