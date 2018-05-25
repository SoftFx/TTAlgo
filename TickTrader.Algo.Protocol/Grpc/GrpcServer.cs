using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
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
                Services = { Lib.BotAgent.BindService(new BotAgentServerImpl(AgentServer)) },
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


        public BotAgentServerImpl(IBotAgentServer botAgent)
        {
            _botAgent = botAgent;
        }


        public override Task<Lib.PackageListResponse> GetPackageList(Lib.PackageListRequest request, ServerCallContext context)
        {
            var res = new Lib.PackageListResponse { Status = Lib.Request.Types.RequestStatus.Success };
            res.Packages.AddRange(_botAgent.GetPackageList().Select(ToGrpc.Convert));
            return Task.FromResult(res);
        }

        public override Task<Lib.AccountListResponse> GetAccountList(Lib.AccountListRequest request, ServerCallContext context)
        {
            var res = new Lib.AccountListResponse { Status = Lib.Request.Types.RequestStatus.Success };
            res.Accounts.AddRange(_botAgent.GetAccountList().Select(ToGrpc.Convert));
            return Task.FromResult(res);
        }

        public override Task<Lib.BotListResponse> GetBotList(Lib.BotListRequest request, ServerCallContext context)
        {
            var res = new Lib.BotListResponse { Status = Lib.Request.Types.RequestStatus.Success };
            res.Bots.AddRange(_botAgent.GetBotList());//.Select(ToGrpc.Convert));
            return Task.FromResult(res);
        }

        public override Task<Lib.LoginResponse> Login(Lib.LoginRequest request, ServerCallContext context)
        {
            return Task.FromResult(new Lib.LoginResponse { MajorVersion = VersionSpec.MajorVersion, MinorVersion = VersionSpec.MinorVersion,
                Status = Lib.Request.Types.RequestStatus.Success, Error = Lib.LoginResponse.Types.LoginError.None });
        }

        public override Task SubscribeToUpdates(Lib.SubscribeToUpdatesRequest request, IServerStreamWriter<Lib.UpdateInfo> responseStream, ServerCallContext context)
        {
            return base.SubscribeToUpdates(request, responseStream, context);
        }

        public override Task<Lib.AccountMetadataInfo> GetAccountMetadata(Lib.AccountKey request, ServerCallContext context)
        {
            return base.GetAccountMetadata(request, context);
        }
    }
}
