using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Server
{
    public class RuntimeManagerModel
    {
        private readonly IActorRef _ref;


        public RuntimeManagerModel(IActorRef actor)
        {
            _ref = actor;
        }


        public Task<PkgRuntimeModel> GetPkgRuntime(string pkgId) => _ref.Ask<PkgRuntimeModel>(new RuntimeManager.PkgRuntimeRequest(pkgId));

        public Task Shutdown() => _ref.Ask(new RuntimeManager.ShutdownRuntimesCmd());

        internal void OnRuntimeStopped(string runtimeId) => _ref.Tell(new RuntimeManager.RuntimeStoppedMsg(runtimeId));

        internal Task<PkgRuntimeModel> ConnectRuntime(string runtimeId, RpcSession session) => _ref.Ask<PkgRuntimeModel>(new RuntimeManager.ConnectRuntimeCmd(runtimeId, session));
    }
}
