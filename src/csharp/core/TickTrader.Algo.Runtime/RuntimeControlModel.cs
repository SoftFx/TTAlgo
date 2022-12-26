using Google.Protobuf.WellKnownTypes;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Runtime
{
    public static class RuntimeControlModel
    {
        public static Task Shutdown(IActorRef actor) => actor.Ask(RuntimeControlActor.ShutdownCmd.Instance);

        public static void MarkObsolete(IActorRef actor) => actor.Tell(RuntimeControlActor.MarkObsoleteCmd.Instance);

        public static Task<bool> ConnectSession(IActorRef actor, RpcSession session) => actor.Ask<bool>(new RuntimeControlActor.ConnectSessionCmd(session));

        public static Task StartExecutor(IActorRef actor, ExecutorConfig config) => actor.Ask(new StartExecutorRequest { Config = config});

        public static Task StopExecutor(IActorRef actor, string executorId) => actor.Ask(new StopExecutorRequest { ExecutorId = executorId });

        public static Task<bool> AttachPlugin(IActorRef actor, string pluginId, IActorRef plugin) => actor.Ask<bool>(new RuntimeControlActor.AttachPluginCmd(pluginId, plugin));

        public static Task<bool> DetachPlugin(IActorRef actor, string pluginId) => actor.Ask<bool>(new RuntimeControlActor.DetachPluginCmd(pluginId));

        public static Task<PluginInfo> GetPluginInfo(IActorRef actor, PluginKey plugin) => actor.Ask<PluginInfo>(new RuntimeControlActor.GetPluginInfoRequest(plugin));

        public static void OnExecutorNotification(IActorRef actor, string executorId, Any payload) => actor.Tell(new RuntimeControlActor.ExecutorNotificationMsg(executorId, payload));


        public class RuntimeCrashedMsg : Singleton<RuntimeCrashedMsg> { }

        public class RuntimeInvalidMsg : Singleton<RuntimeInvalidMsg> { }
    }
}
