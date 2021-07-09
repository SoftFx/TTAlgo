using TickTrader.Algo.Domain;

namespace TickTrader.Algo.ServerControl
{
    public interface IAccessManager
    {
        bool CanGetSnapshot();

        bool CanSubscribeToUpdates();

        bool CanGetApiMetadata();

        bool CanGetMappingsInfo();

        bool CanGetSetupContext();

        bool CanGetAccountMetadata();

        bool CanGetBotList();

        bool CanAddBot();

        bool CanRemoveBot();

        bool CanStartBot();

        bool CanStopBot();

        bool CanChangeBotConfig();

        bool CanGetAccountList();

        bool CanAddAccount();

        bool CanRemoveAccount();

        bool CanChangeAccount();

        bool CanTestAccount();

        bool CanTestAccountCreds();

        bool CanGetPackageList();

        bool CanUploadPackage();

        bool CanRemovePackage();

        bool CanDownloadPackage();

        bool CanGetBotStatus();

        bool CanGetBotLogs();

        bool CanGetAlerts();

        bool CanGetBotFolderInfo(PluginFolderInfo.Types.PluginFolderId folderId);

        bool CanClearBotFolder();

        bool CanDeleteBotFile();

        bool CanUploadBotFile();

        bool CanDownloadBotFile(PluginFolderInfo.Types.PluginFolderId folderId);
    }
}
