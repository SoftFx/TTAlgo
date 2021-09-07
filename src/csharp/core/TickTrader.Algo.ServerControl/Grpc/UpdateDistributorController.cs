using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;

namespace TickTrader.Algo.ServerControl.Grpc
{
    internal static class UpdateDistributorController
    {
        public static Task AddPluginLogsSub(IActorRef actor, ServerSession.Handler session, string pluginId) => actor.Ask(new AddPluginLogsSubRequest(session, pluginId));

        public static Task RemovePluginLogsSub(IActorRef actor, string sessionId, string pluginId) => actor.Ask(new RemovePluginLogsSubRequest(sessionId, pluginId));

        public static Task AddPluginStatusSub(IActorRef actor, ServerSession.Handler session, string pluginId) => actor.Ask(new AddPluginStatusSubRequest(session, pluginId));

        public static Task RemovePluginStatusSub(IActorRef actor, string sessionId, string pluginId) => actor.Ask(new RemovePluginStatusSubRequest(sessionId, pluginId));


        public class AddPluginLogsSubRequest
        {
            public ServerSession.Handler Session { get; }

            public string PluginId { get; }

            public AddPluginLogsSubRequest(ServerSession.Handler session, string pluginId)
            {
                Session = session;
                PluginId = pluginId;
            }
        }

        public class RemovePluginLogsSubRequest
        {
            public string SessionId { get; }

            public string PluginId { get; }

            public RemovePluginLogsSubRequest(string sessionId, string pluginId)
            {
                SessionId = sessionId;
                PluginId = pluginId;
            }
        }

        public class AddPluginStatusSubRequest
        {
            public ServerSession.Handler Session { get; }

            public string PluginId { get; }

            public AddPluginStatusSubRequest(ServerSession.Handler session, string pluginId)
            {
                Session = session;
                PluginId = pluginId;
            }
        }

        public class RemovePluginStatusSubRequest
        {
            public string SessionId { get; }

            public string PluginId { get; }

            public RemovePluginStatusSubRequest(string sessionId, string pluginId)
            {
                SessionId = sessionId;
                PluginId = pluginId;
            }
        }
    }
}
