﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TickTrader.Algo.Server.PublicAPI
{
    public interface IAlgoServerClient
    {
        #region Connection Management

        ClientStates State { get; }

        string LastError { get; }

        IVersionSpec VersionSpec { get; }

        IAccessManager AccessManager { get; }

        bool Only2FAFailed { get; }


        Task Connect(IClientSessionSettings settings);

        Task Disconnect();


        event Action<ClientStates> ClientStateChanged;

        #endregion Connection Management


        #region Other

        Task<AccountMetadataInfo> GetAccountMetadata(AccountMetadataRequest request);

        #endregion Other


        #region Subscriptions Management

        Task SubscribeToPluginStatus(PluginStatusSubscribeRequest request);

        Task SubscribeToPluginLogs(PluginLogsSubscribeRequest request);

        Task UnsubscribeToPluginStatus(PluginStatusUnsubscribeRequest request);

        Task UnsubscribeToPluginLogs(PluginLogsUnsubscribeRequest request);

        #endregion


        #region Account Management

        Task AddAccount(AddAccountRequest request);

        Task RemoveAccount(RemoveAccountRequest request);

        Task ChangeAccount(ChangeAccountRequest request);

        Task<ConnectionErrorInfo> TestAccount(TestAccountRequest request);

        Task<ConnectionErrorInfo> TestAccountCreds(TestAccountCredsRequest request);

        #endregion Account Management


        #region Package Management

        Task UploadPackage(UploadPackageRequest request, string srcPath, IFileProgressListener progressListener);

        Task RemovePackage(RemovePackageRequest request);

        Task DownloadPackage(DownloadPackageRequest request, string dstPath, IFileProgressListener progressListener);

        #endregion Package Management


        #region Plugin Management

        Task AddPlugin(AddPluginRequest request);

        Task RemovePlugin(RemovePluginRequest request);

        Task StartPlugin(StartPluginRequest request);

        Task StopPlugin(StopPluginRequest request);

        Task ChangePluginConfig(ChangePluginConfigRequest request);


        #endregion Plugin Management


        #region Plugin Files Management

        Task<PluginFolderInfo> GetPluginFolderInfo(PluginFolderInfoRequest request);

        Task ClearPluginFolder(ClearPluginFolderRequest request);

        Task DeletePluginFile(DeletePluginFileRequest request);

        Task DownloadPluginFile(DownloadPluginFileRequest request, string dstPath, IFileProgressListener progressListener);

        Task UploadPluginFile(UploadPluginFileRequest request, string srcPath, IFileProgressListener progressListener);

        #endregion Plugin Files Management

        #region AutoUpdate management

        Task<ServerVersionInfo> GetServerVersion(ServerVersionRequest request);

        Task<ServerUpdateList> GetServerUpdateList(ServerUpdateListRequest request);

        Task<UpdateServiceStatusInfo> StartServerUpdate(StartServerUpdateRequest request);

        Task<UpdateServiceStatusInfo> StartCustomUpdate(StartCustomServerUpdateRequest request, string srcPath);

        #endregion AutoUpdate management
    }
}
