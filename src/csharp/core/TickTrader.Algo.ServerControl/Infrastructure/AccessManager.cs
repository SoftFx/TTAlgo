using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.Algo.ServerControl
{
    public enum AccessLevels
    {
        Anonymous = 0,
        Viewer = 1,
        Dealer = 2,
        Admin = 3,
    }


    public static class AccessLevelHelpers
    {
        public static AccessLevels Convert(this LoginResponse.Types.AccessLevel accessLevel)
        {
            switch (accessLevel)
            {
                case LoginResponse.Types.AccessLevel.Viewer:
                    return AccessLevels.Viewer;
                case LoginResponse.Types.AccessLevel.Dealer:
                    return AccessLevels.Dealer;
                case LoginResponse.Types.AccessLevel.Admin:
                    return AccessLevels.Admin;
                default:
                    return AccessLevels.Anonymous;
            }
        }

        public static LoginResponse.Types.AccessLevel Convert(this AccessLevels accessLevel)
        {
            switch (accessLevel)
            {
                case AccessLevels.Viewer:
                    return LoginResponse.Types.AccessLevel.Viewer;
                case AccessLevels.Dealer:
                    return LoginResponse.Types.AccessLevel.Dealer;
                case AccessLevels.Admin:
                    return LoginResponse.Types.AccessLevel.Admin;
                default:
                    return (LoginResponse.Types.AccessLevel)(-1);
            }
        }
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
