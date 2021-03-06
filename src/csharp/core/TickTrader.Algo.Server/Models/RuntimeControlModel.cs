using Google.Protobuf.WellKnownTypes;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Server
{
    public static class RuntimeControlModel
    {
        internal static Task Shutdown(IActorRef actor) => actor.Ask(RuntimeControlActor.ShutdownCmd.Instance);

        internal static void MarkObsolete(IActorRef actor) => actor.Tell(RuntimeControlActor.MarkObsoleteCmd.Instance);

        internal static Task<bool> ConnectSession(IActorRef actor, RpcSession session) => actor.Ask<bool>(new RuntimeControlActor.ConnectSessionCmd(session));

        internal static Task StartExecutor(IActorRef actor, ExecutorConfig config) => actor.Ask(new StartExecutorRequest { Config = config});

        internal static Task StopExecutor(IActorRef actor, string executorId) => actor.Ask(new StopExecutorRequest { ExecutorId = executorId });

        internal static Task<bool> AttachPlugin(IActorRef actor, string pluginId, IActorRef plugin) => actor.Ask<bool>(new RuntimeControlActor.AttachPluginCmd(pluginId, plugin));

        internal static Task<bool> DetachPlugin(IActorRef actor, string pluginId) => actor.Ask<bool>(new RuntimeControlActor.DetachPluginCmd(pluginId));

        internal static Task<PluginInfo> GetPluginInfo(IActorRef actor, PluginKey plugin) => actor.Ask<PluginInfo>(new RuntimeControlActor.GetPluginInfoRequest(plugin));

        internal static void OnExecutorNotification(IActorRef actor, string executorId, Any payload) => actor.Tell(new RuntimeControlActor.ExecutorNotificationMsg(executorId, payload));
    }
}
