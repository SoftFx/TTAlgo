using System;
using System.Collections.Generic;
using System.IO;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;

namespace TickTrader.Algo.Protocol
{
    public interface IBotAgentServer
    {
        #region Initialization

        AccessLevels ValidateCreds(string login, string password);

        List<PackageInfo> GetPackageList();

        List<AccountModelInfo> GetAccountList();

        List<BotModelInfo> GetBotList();

        #endregion Initialization


        #region Updates

        event Action<UpdateInfo<PackageInfo>> PackageUpdated;

        event Action<UpdateInfo<AccountModelInfo>> AccountUpdated;

        event Action<UpdateInfo<BotModelInfo>> BotUpdated;

        event Action<PackageInfo> PackageStateUpdated;

        event Action<BotModelInfo> BotStateUpdated;

        event Action<AccountModelInfo> AccountStateUpdated;

        #endregion Updates


        #region Requests

        ApiMetadataInfo GetApiMetadata();

        MappingCollectionInfo GetMappingsInfo();

        SetupContextInfo GetSetupContext();

        AccountMetadataInfo GetAccountMetadata(AccountKey account);

        void StartBot(string botId);

        void StopBot(string botId);

        void AddBot(AccountKey account, PluginConfig config);

        void RemoveBot(string botId, bool cleanLog, bool cleanAlgoData);

        void ChangeBotConfig(string botId, PluginConfig newConfig);

        void AddAccount(AccountKey account, string password, bool useNewProtocol);

        void RemoveAccount(AccountKey account);

        void ChangeAccount(AccountKey account, string password, bool useNewProtocol);

        ConnectionErrorInfo TestAccount(AccountKey account);

        ConnectionErrorInfo TestAccountCreds(AccountKey account, string password, bool useNewProtocol);

        void RemovePackage(PackageKey package);

        Stream GetPackageReadStream(PackageKey package);

        Stream GetPackageWriteStream(PackageKey package);

        string GetBotStatus(string botId);

        LogRecordInfo[] GetBotLogs(string botId, DateTime lastLogTimeUtc, int maxCount);

        BotFolderInfo GetBotFolderInfo(string botId, BotFolderId folderId);

        void ClearBotFolder(string botId, BotFolderId folderId);

        void DeleteBotFile(string botId, BotFolderId folderId, string fileName);

        Stream GetBotFileReadStream(string botId, BotFolderId folderId, string fileName);

        Stream GetBotFileWriteStream(string botId, BotFolderId folderId, string fileName);

        #endregion Requests
    }
}
