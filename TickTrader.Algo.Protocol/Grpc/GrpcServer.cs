using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using NLog;
using TickTrader.Algo.Common.Info;
using GrpcCore = Grpc.Core;

namespace TickTrader.Algo.Protocol.Grpc
{
    public class GrpcServer : ProtocolServer
    {
        private BotAgentServerImpl _impl;
        private GrpcCore.Server _server;


        public GrpcServer(IBotAgentServer agentServer, IServerSettings settings) : base(agentServer, settings)
        {
        }


        protected override void StartServer()
        {
            _impl = new BotAgentServerImpl(AgentServer, Logger);
            _server = new GrpcCore.Server
            {
                Services = { Lib.BotAgent.BindService(_impl) },
                Ports = { new ServerPort("localhost", Settings.ProtocolSettings.ListeningPort, ServerCredentials.Insecure) },
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
        private ILogger _logger;
        private List<Tuple<TaskCompletionSource<object>, IServerStreamWriter<Lib.UpdateInfo>>> _updateListeners;


        public BotAgentServerImpl(IBotAgentServer botAgent, ILogger logger)
        {
            _botAgent = botAgent;
            _logger = logger;

            _updateListeners = new List<Tuple<TaskCompletionSource<object>, IServerStreamWriter<Lib.UpdateInfo>>>();

            _botAgent.PackageUpdated += OnPackageUpdate;
            _botAgent.AccountUpdated += OnAccountUpdate;
            _botAgent.AccountStateUpdated += OnAccountStateUpdate;
            _botAgent.BotUpdated += OnBotUpdate;
            _botAgent.BotStateUpdated += OnBotStateUpdate;
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


        public override Task<Lib.PackageListResponse> GetPackageList(Lib.PackageListRequest request, ServerCallContext context)
        {
            var res = new Lib.PackageListResponse { Status = Lib.Request.Types.RequestStatus.Success };
            try
            {
                res.Packages.AddRange(_botAgent.GetPackageList().Select(ToGrpc.Convert));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get packages list");
                res.Status = Lib.Request.Types.RequestStatus.InternalServerError;
            }
            return Task.FromResult(res);
        }

        public override Task<Lib.AccountListResponse> GetAccountList(Lib.AccountListRequest request, ServerCallContext context)
        {
            var res = new Lib.AccountListResponse { Status = Lib.Request.Types.RequestStatus.Success };
            try
            {
                res.Accounts.AddRange(_botAgent.GetAccountList().Select(ToGrpc.Convert));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get account list");
                res.Status = Lib.Request.Types.RequestStatus.InternalServerError;
            }
            return Task.FromResult(res);
        }

        public override Task<Lib.BotListResponse> GetBotList(Lib.BotListRequest request, ServerCallContext context)
        {
            var res = new Lib.BotListResponse { Status = Lib.Request.Types.RequestStatus.Success };
            try
            {
                res.Bots.AddRange(_botAgent.GetBotList().Select(ToGrpc.Convert));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get bot list");
                res.Status = Lib.Request.Types.RequestStatus.InternalServerError;
            }
            return Task.FromResult(res);
        }

        public override Task<Lib.LoginResponse> Login(Lib.LoginRequest request, ServerCallContext context)
        {
            return Task.FromResult(new Lib.LoginResponse
            {
                MajorVersion = VersionSpec.MajorVersion,
                MinorVersion = VersionSpec.MinorVersion,
                Status = Lib.Request.Types.RequestStatus.Success,
                Error = Lib.LoginResponse.Types.LoginError.None
            });
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
            var res = new Lib.ApiMetadataResponse { Status = Lib.Request.Types.RequestStatus.Success };
            try
            {
                res.ApiMetadata = _botAgent.GetApiMetadata().Convert();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get api metadata");
                res.Status = Lib.Request.Types.RequestStatus.InternalServerError;
            }
            return Task.FromResult(res);
        }

        public override Task<Lib.MappingsInfoResponse> GetMappingsInfo(Lib.MappingsInfoRequest request, ServerCallContext context)
        {
            var res = new Lib.MappingsInfoResponse { Status = Lib.Request.Types.RequestStatus.Success };
            try
            {
                res.Mappings = _botAgent.GetMappingsInfo().Convert();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get mappings collection");
                res.Status = Lib.Request.Types.RequestStatus.InternalServerError;
            }
            return Task.FromResult(res);
        }

        public override Task<Lib.SetupContextResponse> GetSetupContext(Lib.SetupContextRequest request, ServerCallContext context)
        {
            var res = new Lib.SetupContextResponse { Status = Lib.Request.Types.RequestStatus.Success };
            try
            {
                res.SetupContext = _botAgent.GetSetupContext().Convert();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get setup context");
                res.Status = Lib.Request.Types.RequestStatus.InternalServerError;
            }
            return Task.FromResult(res);
        }

        public override Task<Lib.AccountMetadataResponse> GetAccountMetadata(Lib.AccountMetadataRequest request, ServerCallContext context)
        {
            var res = new Lib.AccountMetadataResponse { Status = Lib.Request.Types.RequestStatus.Success };
            try
            {
                res.AccountMetadata = _botAgent.GetAccountMetadata(request.Account.Convert()).Convert();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get account metadata");
                res.Status = Lib.Request.Types.RequestStatus.InternalServerError;
            }
            return Task.FromResult(res);
        }

        public override Task<Lib.StartBotResponse> StartBot(Lib.StartBotRequest request, ServerCallContext context)
        {
            var res = new Lib.StartBotResponse { Status = Lib.Request.Types.RequestStatus.Success };
            try
            {
                _botAgent.StartBot(request.BotId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to start bot");
                res.Status = Lib.Request.Types.RequestStatus.InternalServerError;
            }
            return Task.FromResult(res);
        }

        public override Task<Lib.StopBotResponse> StopBot(Lib.StopBotRequest request, ServerCallContext context)
        {
            var res = new Lib.StopBotResponse { Status = Lib.Request.Types.RequestStatus.Success };
            try
            {
                _botAgent.StopBot(request.BotId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to stop bot");
                res.Status = Lib.Request.Types.RequestStatus.InternalServerError;
            }
            return Task.FromResult(res);
        }

        public override Task<Lib.AddBotResponse> AddBot(Lib.AddBotRequest request, ServerCallContext context)
        {
            var res = new Lib.AddBotResponse { Status = Lib.Request.Types.RequestStatus.Success };
            try
            {
                _botAgent.AddBot(request.Account.Convert(), request.Config.Convert());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to add bot");
                res.Status = Lib.Request.Types.RequestStatus.InternalServerError;
            }
            return Task.FromResult(res);

        }

        public override Task<Lib.RemoveBotResponse> RemoveBot(Lib.RemoveBotRequest request, ServerCallContext context)
        {
            var res = new Lib.RemoveBotResponse { Status = Lib.Request.Types.RequestStatus.Success };
            try
            {
                _botAgent.RemoveBot(request.BotId, request.CleanLog, request.CleanAlgoData);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to add bot");
                res.Status = Lib.Request.Types.RequestStatus.InternalServerError;
            }
            return Task.FromResult(res);

        }

        public override Task<Lib.ChangeBotConfigResponse> ChangeBotConfig(Lib.ChangeBotConfigRequest request, ServerCallContext context)
        {
            var res = new Lib.ChangeBotConfigResponse { Status = Lib.Request.Types.RequestStatus.Success };
            try
            {
                _botAgent.ChangeBotConfig(request.BotId, request.NewConfig.Convert());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to add bot");
                res.Status = Lib.Request.Types.RequestStatus.InternalServerError;
            }
            return Task.FromResult(res);

        }

        public override Task<Lib.AddAccountResponse> AddAccount(Lib.AddAccountRequest request, ServerCallContext context)
        {
            var res = new Lib.AddAccountResponse { Status = Lib.Request.Types.RequestStatus.Success };
            try
            {
                _botAgent.AddAccount(request.Account.Convert(), request.Password, request.UseNewProtocol);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to add account");
                res.Status = Lib.Request.Types.RequestStatus.InternalServerError;
            }
            return Task.FromResult(res);
        }

        public override Task<Lib.RemoveAccountResponse> RemoveAccount(Lib.RemoveAccountRequest request, ServerCallContext context)
        {
            var res = new Lib.RemoveAccountResponse { Status = Lib.Request.Types.RequestStatus.Success };
            try
            {
                _botAgent.RemoveAccount(request.Account.Convert());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to remove account");
                res.Status = Lib.Request.Types.RequestStatus.InternalServerError;
            }
            return Task.FromResult(res);
        }

        public override Task<Lib.ChangeAccountResponse> ChangeAccount(Lib.ChangeAccountRequest request, ServerCallContext context)
        {
            var res = new Lib.ChangeAccountResponse { Status = Lib.Request.Types.RequestStatus.Success };
            try
            {
                _botAgent.ChangeAccount(request.Account.Convert(), request.Password, request.UseNewProtocol);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to change account");
                res.Status = Lib.Request.Types.RequestStatus.InternalServerError;
            }
            return Task.FromResult(res);
        }

        public override Task<Lib.TestAccountResponse> TestAccount(Lib.TestAccountRequest request, ServerCallContext context)
        {
            var res = new Lib.TestAccountResponse { Status = Lib.Request.Types.RequestStatus.Success };
            try
            {
                res.ErrorInfo = _botAgent.TestAccount(request.Account.Convert()).Convert();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to test account");
                res.Status = Lib.Request.Types.RequestStatus.InternalServerError;
            }
            return Task.FromResult(res);
        }

        public override Task<Lib.TestAccountCredsResponse> TestAccountCreds(Lib.TestAccountCredsRequest request, ServerCallContext context)
        {
            var res = new Lib.TestAccountCredsResponse { Status = Lib.Request.Types.RequestStatus.Success };
            try
            {
                res.ErrorInfo = _botAgent.TestAccountCreds(request.Account.Convert(), request.Password, request.UseNewProtocol).Convert();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to test account creds");
                res.Status = Lib.Request.Types.RequestStatus.InternalServerError;
            }
            return Task.FromResult(res);
        }

        public override Task<Lib.UploadPackageResponse> UploadPackage(Lib.UploadPackageRequest request, ServerCallContext context)
        {
            var res = new Lib.UploadPackageResponse { Status = Lib.Request.Types.RequestStatus.Success };
            try
            {
                _botAgent.UploadPackage(request.FileName, request.PackageBinary.Convert());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to upload package");
                res.Status = Lib.Request.Types.RequestStatus.InternalServerError;
            }
            return Task.FromResult(res);
        }

        public override Task<Lib.RemovePackageResponse> RemovePackage(Lib.RemovePackageRequest request, ServerCallContext context)
        {
            var res = new Lib.RemovePackageResponse { Status = Lib.Request.Types.RequestStatus.Success };
            try
            {
                _botAgent.RemovePackage(request.Package.Convert());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to remove package");
                res.Status = Lib.Request.Types.RequestStatus.InternalServerError;
            }
            return Task.FromResult(res);
        }

        public override Task<Lib.DownloadPackageResponse> DownloadPackage(Lib.DownloadPackageRequest request, ServerCallContext context)
        {
            var res = new Lib.DownloadPackageResponse { Status = Lib.Request.Types.RequestStatus.Success };
            try
            {
                res.PackageBinary = _botAgent.DownloadPackage(request.Package.Convert()).Convert();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to download package");
                res.Status = Lib.Request.Types.RequestStatus.InternalServerError;
            }
            return Task.FromResult(res);
        }

        #region Updates

        private void OnPackageUpdate(UpdateInfo<PackageInfo> update)
        {
            SendUpdate(update.Convert());
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
