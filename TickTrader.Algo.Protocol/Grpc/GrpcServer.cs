using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using NLog;
using TickTrader.Algo.Common.Info;
using GrpcCore = Grpc.Core;

namespace TickTrader.Algo.Protocol.Grpc
{
    public class GrpcServer : ProtocolServer
    {
        private IJwtProvider _jwtProvider;
        private BotAgentServerImpl _impl;
        private GrpcCore.Server _server;


        static GrpcServer()
        {
            CertificateProvider.InitServer();
        }


        public GrpcServer(IBotAgentServer agentServer, IServerSettings settings, IJwtProvider jwtProvider) : base(agentServer, settings)
        {
            _jwtProvider = jwtProvider;
        }


        protected override void StartServer()
        {
            GrpcEnvironment.SetLogger(new GrpcLoggerAdapter(Logger));
            _impl = new BotAgentServerImpl(AgentServer, _jwtProvider, Logger);
            var creds = new SslServerCredentials(new[] { new KeyCertificatePair(CertificateProvider.ServerCertificate, CertificateProvider.ServerKey), }); //,CertificateProvider.RootCertificate, true);
            _server = new GrpcCore.Server
            {
                Services = { Lib.BotAgent.BindService(_impl) },
                Ports = { new ServerPort("localhost", Settings.ProtocolSettings.ListeningPort, creds) },
            };
            _server.Start();
        }

        protected override void StopServer()
        {
            _impl.DisconnectAllClients();
            _server.ShutdownAsync().Wait();
        }
    }


    internal class BotAgentServerImpl : Lib.BotAgent.BotAgentBase
    {
        private IBotAgentServer _botAgent;
        private IJwtProvider _jwtProvider;
        private ILogger _logger;
        private JsonFormatter _messageFormatter;
        private List<Tuple<TaskCompletionSource<object>, IServerStreamWriter<Lib.UpdateInfo>>> _updateListeners;


        public BotAgentServerImpl(IBotAgentServer botAgent, IJwtProvider jwtProvider, ILogger logger)
        {
            _botAgent = botAgent;
            _jwtProvider = jwtProvider;
            _logger = logger;

            _messageFormatter = new JsonFormatter(new JsonFormatter.Settings(true));
            _updateListeners = new List<Tuple<TaskCompletionSource<object>, IServerStreamWriter<Lib.UpdateInfo>>>();

            _botAgent.PackageUpdated += OnPackageUpdate;
            _botAgent.PackageStateUpdated += OnPackageStateUpdate;
            _botAgent.AccountUpdated += OnAccountUpdate;
            _botAgent.AccountStateUpdated += OnAccountStateUpdate;
            _botAgent.BotUpdated += OnBotUpdate;
            _botAgent.BotStateUpdated += OnBotStateUpdate;
        }


        public static Lib.RequestResult CreateSuccessResult(string message = "")
        {
            return new Lib.RequestResult { Status = Lib.RequestResult.Types.RequestStatus.Success, Message = message ?? "" };
        }

        public static Lib.RequestResult CreateErrorResult(string message = "")
        {
            return new Lib.RequestResult { Status = Lib.RequestResult.Types.RequestStatus.InternalServerError, Message = message ?? "" };
        }

        public static Lib.RequestResult CreateErrorResult(Exception ex)
        {
            return CreateErrorResult(ex.Flatten().Message);
        }

        public static Lib.RequestResult CreateUnauthorizedResult(string message = "")
        {
            return new Lib.RequestResult { Status = Lib.RequestResult.Types.RequestStatus.Unauthorized, Message = message ?? "" };
        }

        public static Lib.RequestResult CreateUnauthorizedResult(Exception ex)
        {
            return CreateUnauthorizedResult(ex.Flatten().Message);
        }

        public static Lib.RequestResult CreateRejectResult(string message = "")
        {
            return new Lib.RequestResult { Status = Lib.RequestResult.Types.RequestStatus.Reject, Message = message ?? "" };
        }

        public static Lib.RequestResult CreateRejectResult(Exception ex)
        {
            return CreateRejectResult(ex.Flatten().Message);
        }


        public void DisconnectAllClients()
        {
            lock (_updateListeners)
            {
                foreach (var listener in _updateListeners)
                {
                    listener.Item1.SetCanceled();
                }
                _updateListeners.Clear();
            }
        }


        public override Task<Lib.LoginResponse> Login(Lib.LoginRequest request, ServerCallContext context)
        {
            var res = new Lib.LoginResponse
            {
                MajorVersion = VersionSpec.MajorVersion,
                MinorVersion = VersionSpec.MinorVersion,
                ExecResult = CreateSuccessResult(),
                Error = Lib.LoginResponse.Types.LoginError.None,
                AccessToken = "",
            };

            if (!VersionSpec.CheckClientCompatibility(request.MajorVersion, request.MinorVersion, out var error))
            {
                res.ExecResult = CreateRejectResult(error);
                res.Error = Lib.LoginResponse.Types.LoginError.VersionMismatch;
            }
            else if (!_botAgent.ValidateCreds(request.Login, request.Password))
            {
                res.ExecResult = CreateRejectResult();
                res.Error = Lib.LoginResponse.Types.LoginError.InvalidCredentials;
            }
            else
            {
                try
                {
                    var payload = new JwtPayload
                    {
                        Username = request.Login,
                        SessionId = Guid.NewGuid().ToString(),
                        MinorVersion = Math.Min(request.MinorVersion, VersionSpec.MinorVersion),
                    };
                    res.AccessToken = _jwtProvider.CreateToken(payload);
                }
                catch (Exception ex)
                {
                    res.ExecResult = CreateErrorResult(ex);
                    _logger.Error(ex, $"Failed to create access token: {ex.Message}");
                }
            }

            return Task.FromResult(res);
        }

        public override Task<Lib.PackageListResponse> GetPackageList(Lib.PackageListRequest request, ServerCallContext context)
        {
            var res = new Lib.PackageListResponse { ExecResult = CreateSuccessResult() };
            try
            {
                res.Packages.AddRange(_botAgent.GetPackageList().Select(ToGrpc.Convert));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get packages list");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        public override Task<Lib.AccountListResponse> GetAccountList(Lib.AccountListRequest request, ServerCallContext context)
        {
            var res = new Lib.AccountListResponse { ExecResult = CreateSuccessResult() };
            try
            {
                res.Accounts.AddRange(_botAgent.GetAccountList().Select(ToGrpc.Convert));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get account list");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        public override Task<Lib.BotListResponse> GetBotList(Lib.BotListRequest request, ServerCallContext context)
        {
            var res = new Lib.BotListResponse { ExecResult = CreateSuccessResult() };
            try
            {
                res.Bots.AddRange(_botAgent.GetBotList().Select(ToGrpc.Convert));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get bot list");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        public override Task SubscribeToUpdates(Lib.SubscribeToUpdatesRequest request, IServerStreamWriter<Lib.UpdateInfo> responseStream, ServerCallContext context)
        {
            var tcs = new TaskCompletionSource<object>();
            lock (_updateListeners)
            {
                _updateListeners.Add(new Tuple<TaskCompletionSource<object>, IServerStreamWriter<Lib.UpdateInfo>>(tcs, responseStream));
            }
            return tcs.Task;
        }

        public override Task<Lib.ApiMetadataResponse> GetApiMetadata(Lib.ApiMetadataRequest request, ServerCallContext context)
        {
            var res = new Lib.ApiMetadataResponse { ExecResult = CreateSuccessResult() };
            try
            {
                res.ApiMetadata = _botAgent.GetApiMetadata().Convert();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get api metadata");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        public override Task<Lib.MappingsInfoResponse> GetMappingsInfo(Lib.MappingsInfoRequest request, ServerCallContext context)
        {
            var res = new Lib.MappingsInfoResponse { ExecResult = CreateSuccessResult() };
            try
            {
                res.Mappings = _botAgent.GetMappingsInfo().Convert();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get mappings collection");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        public override Task<Lib.SetupContextResponse> GetSetupContext(Lib.SetupContextRequest request, ServerCallContext context)
        {
            var res = new Lib.SetupContextResponse { ExecResult = CreateSuccessResult() };
            try
            {
                res.SetupContext = _botAgent.GetSetupContext().Convert();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get setup context");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        public override Task<Lib.AccountMetadataResponse> GetAccountMetadata(Lib.AccountMetadataRequest request, ServerCallContext context)
        {
            var res = new Lib.AccountMetadataResponse { ExecResult = CreateSuccessResult() };
            try
            {
                res.AccountMetadata = _botAgent.GetAccountMetadata(request.Account.Convert()).Convert();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get account metadata");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        public override Task<Lib.StartBotResponse> StartBot(Lib.StartBotRequest request, ServerCallContext context)
        {
            var res = new Lib.StartBotResponse { ExecResult = CreateSuccessResult() };
            try
            {
                _botAgent.StartBot(request.BotId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to start bot");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        public override Task<Lib.StopBotResponse> StopBot(Lib.StopBotRequest request, ServerCallContext context)
        {
            var res = new Lib.StopBotResponse { ExecResult = CreateSuccessResult() };
            try
            {
                _botAgent.StopBot(request.BotId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to stop bot");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        public override Task<Lib.AddBotResponse> AddBot(Lib.AddBotRequest request, ServerCallContext context)
        {
            var res = new Lib.AddBotResponse { ExecResult = CreateSuccessResult() };
            try
            {
                _botAgent.AddBot(request.Account.Convert(), request.Config.Convert());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to add bot");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);

        }

        public override Task<Lib.RemoveBotResponse> RemoveBot(Lib.RemoveBotRequest request, ServerCallContext context)
        {
            var res = new Lib.RemoveBotResponse { ExecResult = CreateSuccessResult() };
            try
            {
                _botAgent.RemoveBot(request.BotId, request.CleanLog, request.CleanAlgoData);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to add bot");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);

        }

        public override Task<Lib.ChangeBotConfigResponse> ChangeBotConfig(Lib.ChangeBotConfigRequest request, ServerCallContext context)
        {
            var res = new Lib.ChangeBotConfigResponse { ExecResult = CreateSuccessResult() };
            try
            {
                _botAgent.ChangeBotConfig(request.BotId, request.NewConfig.Convert());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to add bot");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);

        }

        public override Task<Lib.AddAccountResponse> AddAccount(Lib.AddAccountRequest request, ServerCallContext context)
        {
            var res = new Lib.AddAccountResponse { ExecResult = CreateSuccessResult() };
            try
            {
                _botAgent.AddAccount(request.Account.Convert(), request.Password, request.UseNewProtocol);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to add account");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        public override Task<Lib.RemoveAccountResponse> RemoveAccount(Lib.RemoveAccountRequest request, ServerCallContext context)
        {
            var res = new Lib.RemoveAccountResponse { ExecResult = CreateSuccessResult() };
            try
            {
                _botAgent.RemoveAccount(request.Account.Convert());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to remove account");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        public override Task<Lib.ChangeAccountResponse> ChangeAccount(Lib.ChangeAccountRequest request, ServerCallContext context)
        {
            var res = new Lib.ChangeAccountResponse { ExecResult = CreateSuccessResult() };
            try
            {
                _botAgent.ChangeAccount(request.Account.Convert(), request.Password, request.UseNewProtocol);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to change account");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        public override Task<Lib.TestAccountResponse> TestAccount(Lib.TestAccountRequest request, ServerCallContext context)
        {
            var res = new Lib.TestAccountResponse { ExecResult = CreateSuccessResult() };
            try
            {
                res.ErrorInfo = _botAgent.TestAccount(request.Account.Convert()).Convert();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to test account");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        public override Task<Lib.TestAccountCredsResponse> TestAccountCreds(Lib.TestAccountCredsRequest request, ServerCallContext context)
        {
            var res = new Lib.TestAccountCredsResponse { ExecResult = CreateSuccessResult() };
            try
            {
                res.ErrorInfo = _botAgent.TestAccountCreds(request.Account.Convert(), request.Password, request.UseNewProtocol).Convert();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to test account creds");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        public override Task<Lib.UploadPackageResponse> UploadPackage(Lib.UploadPackageRequest request, ServerCallContext context)
        {
            var res = new Lib.UploadPackageResponse { ExecResult = CreateSuccessResult() };
            try
            {
                _botAgent.UploadPackage(request.FileName, request.PackageBinary.Convert());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to upload package");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        public override Task<Lib.RemovePackageResponse> RemovePackage(Lib.RemovePackageRequest request, ServerCallContext context)
        {
            var res = new Lib.RemovePackageResponse { ExecResult = CreateSuccessResult() };
            try
            {
                _botAgent.RemovePackage(request.Package.Convert());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to remove package");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        public override Task<Lib.DownloadPackageResponse> DownloadPackage(Lib.DownloadPackageRequest request, ServerCallContext context)
        {
            var res = new Lib.DownloadPackageResponse { ExecResult = CreateSuccessResult() };
            try
            {
                res.PackageBinary = _botAgent.DownloadPackage(request.Package.Convert()).Convert();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to download package");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        #region Updates

        private void OnPackageUpdate(UpdateInfo<PackageInfo> update)
        {
            SendUpdate(update.Convert());
        }

        private void OnPackageStateUpdate(PackageInfo package)
        {
            SendUpdate(new UpdateInfo<PackageInfo> { Type = UpdateType.Replaced, Value = package }.ConvertStateUpdate());
        }

        private void OnAccountUpdate(UpdateInfo<AccountModelInfo> update)
        {
            SendUpdate(update.Convert());
        }

        private void OnAccountStateUpdate(AccountModelInfo account)
        {
            SendUpdate(new UpdateInfo<AccountModelInfo> { Type = UpdateType.Replaced, Value = account }.ConvertStateUpdate());
        }

        private void OnBotUpdate(UpdateInfo<BotModelInfo> update)
        {
            SendUpdate(update.Convert());
        }

        private void OnBotStateUpdate(BotModelInfo bot)
        {
            SendUpdate(new UpdateInfo<BotModelInfo> { Type = UpdateType.Replaced, Value = bot }.ConvertStateUpdate());
        }

        private void SendUpdate(Lib.UpdateInfo update)
        {
            lock (_updateListeners)
            {
                try
                {
                    foreach (var listener in _updateListeners)
                    {
                        try
                        {
                            listener.Item2.WriteAsync(update);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Failed to send update");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to multicast update");
                }
            }
        }

        #endregion
    }
}
