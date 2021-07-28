using Google.Protobuf;
using System.Threading.Tasks;
using TickTrader.Algo.Account;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Server
{
    internal class AlgoServerPrivate : IRpcHost
    {
        private readonly IActorRef _server, _eventBus;


        public string Address { get; set; }

        public int BoundPort { get; set; }

        public EnvService Env { get; }

        public ServerStateModel SavedState { get; }

        public AlertManagerModel Alerts { get; }

        public ConnectionOptions AccountOptions { get; set; }


        public AlgoServerPrivate(IActorRef server, EnvService env, IActorRef eventBus, ServerStateModel savedState, AlertManagerModel alerts)
        {
            _server = server;
            Env = env;
            _eventBus = eventBus;
            SavedState = savedState;
            Alerts = alerts;
        }


        internal void SendUpdate(IMessage update) => _eventBus.Tell(update);

        internal Task<bool> LockPkgRef(string pkgRefId) => _server.Ask<bool>(new LockPkgRefCmd(pkgRefId));

        internal void ReleasePkgRef(string pkgRefId) => _server.Tell(new ReleasePkgRefCmd(pkgRefId));

        internal void OnRuntimeStopped(string runtimeId) => _server.Tell(new RuntimeStoppedMsg(runtimeId));

        internal Task<PkgRuntimeModel> GetPkgRuntime(string pkgId) => _server.Ask<PkgRuntimeModel>(new PkgRuntimeRequest(pkgId));

        internal Task<PkgRuntimeModel> ConnectRuntime(string runtimeId, RpcSession session) => _server.Ask<PkgRuntimeModel>(new ConnectRuntimeCmd(runtimeId, session));

        internal Task<AccountControlModel> GetAccountControl(string accId) => _server.Ask<AccountControlModel>(new AccountControlRequest(accId));


        #region IRpcHost implementation

        ProtocolSpec IRpcHost.Resolve(ProtocolSpec protocol, out string error)
        {
            error = string.Empty;
            return protocol;
        }

        IRpcHandler IRpcHost.GetRpcHandler(ProtocolSpec protocol)
        {
            switch (protocol.Url)
            {
                case KnownProtocolUrls.RuntimeV1:
                    return new ServerRuntimeV1Handler(this);
            }
            return null;
        }

        #endregion IRpcHost implementation


        internal class LockPkgRefCmd
        {
            public string Id { get; }

            public LockPkgRefCmd(string id)
            {
                Id = id;
            }
        }

        internal class ReleasePkgRefCmd
        {
            public string Id { get; }

            public ReleasePkgRefCmd(string id)
            {
                Id = id;
            }
        }

        internal class PkgRuntimeRequest
        {
            public string PkgId { get; }

            public PkgRuntimeRequest(string pkgId)
            {
                PkgId = pkgId;
            }
        }

        internal class RuntimeStoppedMsg
        {
            public string Id { get; }

            public RuntimeStoppedMsg(string id)
            {
                Id = id;
            }
        }

        internal class ConnectRuntimeCmd
        {
            public string Id { get; }

            public RpcSession Session { get; }

            public ConnectRuntimeCmd(string id, RpcSession session)
            {
                Id = id;
                Session = session;
            }
        }

        internal class AccountControlRequest
        {
            public string Id { get; }

            public AccountControlRequest(string id)
            {
                Id = id;
            }
        }
    }
}
