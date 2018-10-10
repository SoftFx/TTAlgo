using Machinarium.State;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;

namespace TickTrader.Algo.Protocol
{
    public enum ClientStates { Offline, Online, Connecting, Disconnecting, LoggingIn, LoggingOut, Initializing, Deinitializing };


    public enum ClientEvents { Started, Connected, Disconnected, ConnectionError, LoggedIn, LoggedOut, LoginReject, Initialized, Deinitialized, LogoutRequest }


    public abstract class ProtocolClient
    {
        public const int DefaultRequestTimeout = 10;


        protected ILogger Logger { get; set; }

        protected StateMachine<ClientStates> StateMachine { get; }

        protected IBotAgentClient AgentClient { get; }


        public IClientSessionSettings SessionSettings { get; protected set; }

        public ClientStates State => StateMachine.Current;

        public string LastError { get; protected set; }

        public VersionSpec VersionSpec { get; private set; }

        public AccessManager AccessManager { get; private set; }


        public event Action Connecting = delegate { };
        public event Action Connected = delegate { };
        public event Action Disconnecting = delegate { };
        public event Action Disconnected = delegate { };


        public ProtocolClient(IBotAgentClient agentClient)
        {
            AgentClient = agentClient;

            VersionSpec = new VersionSpec();
            AccessManager = new AccessManager(AccessLevels.Viewer);

            StateMachine = new StateMachine<ClientStates>(ClientStates.Offline);

            StateMachine.StateChanged += StateMachineOnStateChanged;
            StateMachine.EventFired += StateMachineOnEventFired;

            StateMachine.AddTransition(ClientStates.Offline, ClientEvents.Started, ClientStates.Connecting);
            StateMachine.AddTransition(ClientStates.Connecting, ClientEvents.Connected, ClientStates.LoggingIn);
            StateMachine.AddTransition(ClientStates.Connecting, ClientEvents.ConnectionError, ClientStates.Deinitializing);
            StateMachine.AddTransition(ClientStates.LoggingIn, ClientEvents.LoggedIn, ClientStates.Initializing);
            StateMachine.AddTransition(ClientStates.LoggingIn, ClientEvents.LoginReject, ClientStates.Disconnecting);
            StateMachine.AddTransition(ClientStates.LoggingIn, ClientEvents.ConnectionError, ClientStates.Deinitializing);
            StateMachine.AddTransition(ClientStates.LoggingIn, ClientEvents.Disconnected, ClientStates.Deinitializing);
            StateMachine.AddTransition(ClientStates.Initializing, ClientEvents.Initialized, ClientStates.Online);
            StateMachine.AddTransition(ClientStates.Initializing, ClientEvents.ConnectionError, ClientStates.Deinitializing);
            StateMachine.AddTransition(ClientStates.Initializing, ClientEvents.Disconnected, ClientStates.Deinitializing);
            StateMachine.AddTransition(ClientStates.Initializing, ClientEvents.LoggedOut, ClientStates.Disconnecting);
            StateMachine.AddTransition(ClientStates.Initializing, ClientEvents.LogoutRequest, ClientStates.LoggingOut);
            StateMachine.AddTransition(ClientStates.Online, ClientEvents.ConnectionError, ClientStates.Deinitializing);
            StateMachine.AddTransition(ClientStates.Online, ClientEvents.Disconnected, ClientStates.Deinitializing);
            StateMachine.AddTransition(ClientStates.Online, ClientEvents.LoggedOut, ClientStates.Disconnecting);
            StateMachine.AddTransition(ClientStates.Online, ClientEvents.LogoutRequest, ClientStates.LoggingOut);
            StateMachine.AddTransition(ClientStates.LoggingOut, ClientEvents.LoggedOut, ClientStates.Disconnecting);
            StateMachine.AddTransition(ClientStates.LoggingOut, ClientEvents.ConnectionError, ClientStates.Deinitializing);
            StateMachine.AddTransition(ClientStates.LoggingOut, ClientEvents.Disconnected, ClientStates.Deinitializing);
            StateMachine.AddTransition(ClientStates.Disconnecting, ClientEvents.Disconnected, ClientStates.Deinitializing);
            StateMachine.AddTransition(ClientStates.Disconnecting, ClientEvents.ConnectionError, ClientStates.Deinitializing);
            StateMachine.AddTransition(ClientStates.Deinitializing, ClientEvents.Deinitialized, ClientStates.Offline);

            StateMachine.OnEnter(ClientStates.Connecting, StartConnecting);
            StateMachine.OnEnter(ClientStates.LoggingIn, SendLogin);
            StateMachine.OnEnter(ClientStates.Initializing, Init);
            StateMachine.OnEnter(ClientStates.LoggingOut, SendLogout);
            StateMachine.OnEnter(ClientStates.Disconnecting, SendDisconnect);
            StateMachine.OnEnter(ClientStates.Deinitializing, DeInit);
        }


        public void TriggerConnect(IClientSessionSettings settings)
        {
            StateMachine.SyncContext.Synchronized(() =>
            {
                if (StateMachine.Current != ClientStates.Offline)
                    throw new Exception($"Cannot connect! Client is in state {State}");

                SessionSettings = settings;

                StateMachine.PushEvent(ClientEvents.Started);
            });
        }

        public Task Connect(IClientSessionSettings settings)
        {
            TriggerConnect(settings);
            return StateMachine.AsyncWait(s => s == ClientStates.Online || s == ClientStates.Offline);
        }

        public void TriggerDisconnect()
        {
            StateMachine.PushEvent(ClientEvents.LogoutRequest);
        }

        public Task Disconnect()
        {
            TriggerDisconnect();
            return StateMachine.AsyncWait(ClientStates.Offline);
        }


        protected abstract void StartClient();

        protected abstract void StopClient();

        protected abstract void SendLogin();

        protected abstract void SendLogout();

        protected abstract void SendDisconnect();

        protected abstract void Init();


        protected void OnConnected()
        {
            StateMachine.PushEvent(ClientEvents.Connected);
        }

        protected void OnDisconnected()
        {
            StateMachine.PushEvent(ClientEvents.Disconnected);
        }

        protected void OnConnectionError(string text)
        {
            LastError = $"Connection error: {text}";
            StateMachine.PushEvent(ClientEvents.ConnectionError);
        }

        protected void OnLogin(int serverMajorVersion, int serverMinorVersion, AccessLevels accessLevel)
        {
            VersionSpec = new VersionSpec(serverMinorVersion);
            AccessManager = new AccessManager(accessLevel);
            Logger.Info($"Client version - {VersionSpec.LatestVersion}; Server version - {serverMajorVersion}.{serverMinorVersion}");
            Logger.Info($"Current version set to {VersionSpec.CurrentVersionStr}");
            StateMachine.PushEvent(ClientEvents.LoggedIn);
        }

        protected void OnLoginReject(string reason)
        {
            LastError = reason;
            StateMachine.PushEvent(ClientEvents.LoginReject);
        }

        protected void OnLogout(string reason)
        {
            //LastError = reason;
            StateMachine.PushEvent(ClientEvents.LoggedOut);
        }

        protected void OnSubscribed()
        {
            StateMachine.PushEvent(ClientEvents.Initialized);
        }


        #region Connection routine

        private void StartConnecting()
        {
            Logger = LoggerHelper.GetLogger(GetType().Name, SessionSettings.ProtocolSettings.LogDirectoryName, SessionSettings.ServerAddress);

            LastError = null;

            try
            {
                StartClient();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to start client");
                OnConnectionError("Client failed to start");
            }
        }

        private void DeInit()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    StopClient();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to stop client");
                }

                StateMachine.PushEvent(ClientEvents.Deinitialized);
            });
        }

        private void StateMachineOnStateChanged(ClientStates from, ClientStates to)
        {
            Logger?.Debug($"STATE {from} -> {to}");
            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (to == ClientStates.Connecting)
                    {
                        Connecting();
                    }
                    if (to == ClientStates.Online)
                    {
                        Connected();
                    }
                    if (from == ClientStates.Online)
                    {
                        Disconnecting();
                    }
                    if (to == ClientStates.Offline)
                    {
                        Disconnected();
                    }
                }
                catch (Exception ex)
                {
                    Logger?.Error(ex, $"Connection event failed: {ex.Message}");
                }
            });
        }

        private void StateMachineOnEventFired(object e)
        {
            Logger?.Debug($"EVENT {e}");
        }

        #endregion Connection routine


        #region Requests

        public abstract Task<ApiMetadataInfo> GetApiMetadata();

        public abstract Task<MappingCollectionInfo> GetMappingsInfo();

        public abstract Task<SetupContextInfo> GetSetupContext();

        public abstract Task<AccountMetadataInfo> GetAccountMetadata(AccountKey account);

        public abstract Task<List<BotModelInfo>> GetBotList();

        public abstract Task AddBot(AccountKey account, PluginConfig config);

        public abstract Task RemoveBot(string botId, bool cleanLog = false, bool cleanAlgoData = false);

        public abstract Task StartBot(string botId);

        public abstract Task StopBot(string botId);

        public abstract Task ChangeBotConfig(string botId, PluginConfig newConfig);

        public abstract Task<List<AccountModelInfo>> GetAccountList();

        public abstract Task AddAccount(AccountKey account, string password, bool useNewProtocol);

        public abstract Task RemoveAccount(AccountKey account);

        public abstract Task ChangeAccount(AccountKey account, string password, bool useNewProtocol);

        public abstract Task<ConnectionErrorInfo> TestAccount(AccountKey account);

        public abstract Task<ConnectionErrorInfo> TestAccountCreds(AccountKey account, string password, bool useNewProtocol);

        public abstract Task<List<PackageInfo>> GetPackageList();

        public abstract Task UploadPackage(PackageKey package, string srcPath, int chunkSize, int offset, IFileProgressListener progressListener);

        public abstract Task RemovePackage(PackageKey package);

        public abstract Task DownloadPackage(PackageKey package, string dstPath, int chunkSize, int offset, IFileProgressListener progressListener);

        public abstract Task<string> GetBotStatus(string botId);

        public abstract Task<LogRecordInfo[]> GetBotLogs(string botId, DateTime lastLogTimeUtc, int maxCount);

        public abstract Task<BotFolderInfo> GetBotFolderInfo(string botId, BotFolderId folderId);

        public abstract Task ClearBotFolder(string botId, BotFolderId folderId);

        public abstract Task DeleteBotFile(string botId, BotFolderId folderId, string fileName);

        public abstract Task DownloadBotFile(string botId, BotFolderId folderId, string fileName, string dstPath, int chunkSize, int offset, IFileProgressListener progressListener);

        public abstract Task UploadBotFile(string botId, BotFolderId folderId, string fileName, string srcPath, int chunkSize, int offset, IFileProgressListener progressListener);

        #endregion Requests
    }
}
