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

        bool CanGetAlerts();


        #region Plugin management permissions

        bool CanAddBot();

        bool CanRemoveBot();

        bool CanStartBot();

        bool CanStopBot();

        bool CanChangeBotConfig();

        bool CanGetAccountList();

        bool CanGetBotStatus();

        bool CanGetBotLogs();

        #endregion


        #region Account management permissions

        bool CanAddAccount();

        bool CanRemoveAccount();

        bool CanChangeAccount();

        bool CanTestAccount();

        bool CanTestAccountCreds();

        #endregion


        #region Package management permissions

        bool CanGetPackageList();

        bool CanUploadPackage();

        bool CanRemovePackage();

        bool CanDownloadPackage();

        #endregion


        #region Plugin Files management permissions

        bool CanGetBotFolderInfo(PluginFolderInfo.Types.PluginFolderId folderId);

        bool CanClearBotFolder();

        bool CanDeleteBotFile();

        bool CanUploadBotFile();

        bool CanDownloadBotFile(PluginFolderInfo.Types.PluginFolderId folderId);

        #endregion
    }
}
