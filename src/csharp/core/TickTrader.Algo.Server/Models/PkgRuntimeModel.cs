using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Server
{
    public class PkgRuntimeModel
    {
        private readonly IActorRef _ref;


        public PkgRuntimeModel(IActorRef actor)
        {
            _ref = actor;
        }


        public Task<bool> Start() => _ref.Ask<bool>(new PkgRuntimeActor.StartRuntimeCmd());

        public Task Stop(string reason) => _ref.Ask(new PkgRuntimeActor.StopRuntimeCmd(reason));

        public Task<RuntimeConfig> GetConfig() => _ref.Ask<RuntimeConfig>(new RuntimeConfigRequest());


        internal void MarkForShutdown() => _ref.Tell(new PkgRuntimeActor.MarkForShutdownCmd());

        internal Task<bool> OnConnect(RpcSession session) => _ref.Ask<bool>(new PkgRuntimeActor.ConnectSessionCmd(session));

        internal Task<bool> AttachAccount(AttachAccountRequest request) => _ref.Ask<bool>(request);

        internal Task<bool> DetachAccount(DetachAccountRequest request) => _ref.Ask<bool>(request);

        internal Task StartExecutor(StartExecutorRequest request) => _ref.Ask(request);

        internal Task StopExecutor(StopExecutorRequest request) => _ref.Ask(request);

        internal void OnExecutorStopped(string executorId) => _ref.Tell(new PkgRuntimeActor.ExecutorStoppedMsg(executorId));

        internal Task<PluginInfo> GetPluginInfo(PluginKey plugin) => _ref.Ask<PluginInfo>(new PkgRuntimeActor.GetPluginInfoRequest(plugin));

        internal Task<ExecutorModel> CreateExecutor(string executorId, ExecutorConfig config) => _ref.Ask<ExecutorModel>(new PkgRuntimeActor.CreateExecutorCmd(executorId, config));
    }
}
