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
        private GrpcCore.Server _server;


        public GrpcServer(IBotAgentServer agentServer, IServerSettings settings) : base(agentServer, settings)
        {
        }


        protected override void StartServer()
        {
            _server = new GrpcCore.Server
            {
                Services = { Lib.BotAgent.BindService(new BotAgentServerImpl(AgentServer, Logger)) },
                Ports = { new ServerPort("localhost", Settings.ProtocolSettings.ListeningPort, ServerCredentials.Insecure) },
            };
            _server.Start();
        }

        protected override void StopServer()
        {
            _server.ShutdownAsync().Wait();
        }
    }


    internal class BotAgentServerImpl : Lib.BotAgent.BotAgentBase
    {
        private IBotAgentServer _botAgent;
        private ILogger _logger;
        private List<IServerStreamWriter<Lib.UpdateInfo>> _updateListeners;


        public BotAgentServerImpl(IBotAgentServer botAgent, ILogger logger)
        {
            _botAgent = botAgent;
            _logger = logger;

            _updateListeners = new List<IServerStreamWriter<Lib.UpdateInfo>>();

            _botAgent.PackageUpdated += OnPackageUpdate;
            _botAgent.AccountUpdated += OnAccountUpdate;
            _botAgent.AccountStateUpdated += OnAccountStateUpdate;
            _botAgent.BotUpdated += OnBotUpdate;
            _botAgent.BotStateUpdated += OnBotStateUpdate;
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
            lock (_updateListeners)
            {
                _updateListeners.Add(responseStream);
            }
            return new TaskCompletionSource<object>().Task;
            //return Task.FromResult(this);
        }

        public override Task<Lib.AccountMetadataInfo> GetAccountMetadata(Lib.AccountKey request, ServerCallContext context)
        {
            return base.GetAccountMetadata(request, context);
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
                        listener.WriteAsync(update);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to send update");
                }
            }
        }

        #endregion
    }
}
