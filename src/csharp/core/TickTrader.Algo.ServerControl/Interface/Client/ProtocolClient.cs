using Machinarium.State;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.Algo.ServerControl
{
    public enum ClientStates { Offline, Online, Connecting, Disconnecting, LoggingIn, LoggingOut, Initializing, Deinitializing };


    public enum ClientEvents { Started, Connected, Disconnected, ConnectionError, LoggedIn, LoggedOut, LoginReject, Initialized, Deinitialized, LogoutRequest }


    public abstract class ProtocolClient : IAlgoServerClient
    {
        public const int DefaultRequestTimeout = 10;


        protected ILogger Logger { get; set; }

        protected StateMachine<ClientStates> StateMachine { get; }

        protected IAlgoServerEventHandler AlgoClient { get; }

        protected ClientSessionSettings SessionSettings { get; set; }


        public ClientStates State => StateMachine.Current;

        public string LastError { get; protected set; }

        public VersionSpec VersionSpec { get; private set; }

        public AccessManager AccessManager { get; private set; }


        IVersionSpec IAlgoServerClient.VersionSpec => VersionSpec;

        IAccessManager IAlgoServerClient.AccessManager => AccessManager;


        public event Action Connecting = delegate { };
        public event Action Connected = delegate { };
        public event Action Disconnecting = delegate { };
        public event Action Disconnected = delegate { };


        public ProtocolClient(IAlgoServerEventHandler algoClient)
        {
            AlgoClient = algoClient;

            VersionSpec = new VersionSpec();
            AccessManager = new AccessManager(ClientClaims.Types.AccessLevel.Anonymous);

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


        public static ProtocolClient Create(IAlgoServerEventHandler client)
        {
            return new Grpc.GrpcClient(client);
        }


        public void TriggerConnect(ClientSessionSettings settings)
        {
            StateMachine.SyncContext.Synchronized(() =>
            {
                if (StateMachine.Current != ClientStates.Offline)
                    throw new Exception($"Cannot connect! Client is in state {State}");

                SessionSettings = settings;

                StateMachine.PushEvent(ClientEvents.Started);
            });
        }

        public Task Connect(ClientSessionSettings settings)
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


        public abstract void StartClient();

        public abstract void StopClient();

        public abstract void SendLogin();

        public abstract void SendLogout();

        public abstract void SendDisconnect();

        public abstract void Init();


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

        protected void OnLogin(int serverMajorVersion, int serverMinorVersion, ClientClaims.Types.AccessLevel accessLevel)
        {
            VersionSpec = new VersionSpec(serverMinorVersion);
            AccessManager = new AccessManager(accessLevel);
            AlgoClient.AccessLevelChanged();
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
            AccessManager = new AccessManager(ClientClaims.Types.AccessLevel.Anonymous);
            AlgoClient.AccessLevelChanged();
            StateMachine.PushEvent(ClientEvents.LoggedOut);
        }

        protected void OnSubscribed()
        {
            StateMachine.PushEvent(ClientEvents.Initialized);
        }


        #region Connection routine

        private void StartConnecting()
        {
            Logger = LoggerHelper.GetLogger(GetType().Name, System.IO.Path.Combine(SessionSettings.LogDirectory, GetType().Name), SessionSettings.ServerAddress);

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

        public abstract Task<AccountMetadataInfo> GetAccountMetadata(AccountMetadataRequest request);

        public abstract Task<List<PluginModelInfo>> GetPluginList();

        public abstract Task AddPlugin(AddPluginRequest request);

        public abstract Task RemovePlugin(RemovePluginRequest request);

        public abstract Task StartPlugin(StartPluginRequest request);

        public abstract Task StopPlugin(StopPluginRequest request);

        public abstract Task ChangePluginConfig(ChangePluginConfigRequest request);

        public abstract Task<List<AccountModelInfo>> GetAccountList();

        public abstract Task AddAccount(AddAccountRequest request);

        public abstract Task RemoveAccount(RemoveAccountRequest request);

        public abstract Task ChangeAccount(ChangeAccountRequest request);

        public abstract Task<ConnectionErrorInfo> TestAccount(TestAccountRequest request);

        public abstract Task<ConnectionErrorInfo> TestAccountCreds(TestAccountCredsRequest request);

        public abstract Task<List<PackageInfo>> GetPackageList();

        public abstract Task UploadPackage(UploadPackageRequest request, string srcPath, IFileProgressListener progressListener);

        public abstract Task RemovePackage(RemovePackageRequest request);

        public abstract Task DownloadPackage(DownloadPackageRequest request, string dstPath, IFileProgressListener progressListener);

        public abstract Task<string> GetPluginStatus(PluginStatusRequest request);

        public abstract Task<LogRecordInfo[]> GetPluginLogs(PluginLogsRequest request);

        public abstract Task<AlertRecordInfo[]> GetAlerts(PluginAlertsRequest request);

        public abstract Task<PluginFolderInfo> GetPluginFolderInfo(PluginFolderInfoRequest request);

        public abstract Task ClearPluginFolder(ClearPluginFolderRequest request);

        public abstract Task DeletePluginFile(DeletePluginFileRequest request);

        public abstract Task DownloadPluginFile(DownloadPluginFileRequest request, string dstPath, IFileProgressListener progressListener);

        public abstract Task UploadPluginFile(UploadPluginFileRequest request, string srcPath, IFileProgressListener progressListener);

        #endregion Requests
    }
}
