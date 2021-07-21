using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Server.PublicAPI
{
    public interface IAccessManager
    {
        bool CanGetAccountMetadata();

        bool CanGetAlerts();


        #region Account management permissions

        bool CanAddAccount();

        bool CanRemoveAccount();

        bool CanChangeAccount();

        bool CanTestAccount();

        bool CanTestAccountCreds();

        #endregion


        #region Package management permissions

        bool CanUploadPackage();

        bool CanRemovePackage();

        bool CanDownloadPackage();

        #endregion


        #region Plugin management permissions

        bool CanAddPlugin();

        bool CanRemovePlugin();

        bool CanStartPlugin();

        bool CanStopPlugin();

        bool CanChangePluginConfig();

        bool CanGetPluginStatus();

        bool CanGetPluginLogs();

        #endregion


        #region Plugin Files management permissions

        bool CanGetBotFolderInfo(PluginFolderInfo.Types.PluginFolderId folderId);

        bool CanClearBotFolder();

        bool CanDeleteBotFile();

        bool CanDownloadBotFile(PluginFolderInfo.Types.PluginFolderId folderId);

        bool CanUploadBotFile();

        #endregion
    }
}
