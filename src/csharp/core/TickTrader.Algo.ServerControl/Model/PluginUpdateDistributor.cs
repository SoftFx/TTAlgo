using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;

namespace TickTrader.Algo.ServerControl.Model
{
    internal static class PluginUpdateDistributor
    {
        public static Task AddPluginLogsSub(IActorRef actor, SessionHandler session, string pluginId) => actor.Ask(new AddPluginLogsSubRequest(session, pluginId));

        public static Task RemovePluginLogsSub(IActorRef actor, string sessionId, string pluginId) => actor.Ask(new RemovePluginLogsSubRequest(sessionId, pluginId));

        public static Task AddPluginStatusSub(IActorRef actor, SessionHandler session, string pluginId) => actor.Ask(new AddPluginStatusSubRequest(session, pluginId));

        public static Task RemovePluginStatusSub(IActorRef actor, string sessionId, string pluginId) => actor.Ask(new RemovePluginStatusSubRequest(sessionId, pluginId));


        public class AddPluginLogsSubRequest
        {
            public SessionHandler Session { get; }

            public string PluginId { get; }

            public AddPluginLogsSubRequest(SessionHandler session, string pluginId)
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
            public SessionHandler Session { get; }

            public string PluginId { get; }

            public AddPluginStatusSubRequest(SessionHandler session, string pluginId)
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
