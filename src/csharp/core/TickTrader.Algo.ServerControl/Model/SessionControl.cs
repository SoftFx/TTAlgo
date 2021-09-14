using Grpc.Core;
using NLog;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Server.PublicAPI;

namespace TickTrader.Algo.ServerControl.Model
{
    internal static class SessionControl
    {
        public static Task Shutdown(IActorRef actor) => actor.Ask(ShutdownCmd.Instance);

        public static Task AddSession(IActorRef actor, SessionInfo session, LogFactory logFactory)
            => actor.Ask<SessionInfo>(new AddSessionRequest(session, logFactory));

        public static Task<SessionInfo> GetSession(IActorRef actor, string sessionId) => actor.Ask<SessionInfo>(new SessionInfoRequest(sessionId));

        public static Task RemoveSession(IActorRef actor, string sessionId) => actor.Ask(new RemoveSessionCmd(sessionId));

        public static Task<Task> OpenUpdatesChannel(IActorRef actor, string sessionId, IServerStreamWriter<UpdateInfo> networkStream)
            => actor.Ask<Task>(new OpenUpdatesChannelCmd(sessionId, networkStream));

        public static Task AddPluginLogsSub(IActorRef actor, string sessionId, string pluginId) => actor.Ask(new AddPluginLogsSubRequest(sessionId, pluginId));

        public static Task RemovePluginLogsSub(IActorRef actor, string sessionId, string pluginId) => actor.Ask(new RemovePluginLogsSubRequest(sessionId, pluginId));

        public static Task AddPluginStatusSub(IActorRef actor, string sessionId, string pluginId) => actor.Ask(new AddPluginStatusSubRequest(sessionId, pluginId));

        public static Task RemovePluginStatusSub(IActorRef actor, string sessionId, string pluginId) => actor.Ask(new RemovePluginStatusSubRequest(sessionId, pluginId));


        public class ShutdownCmd : Singleton<ShutdownCmd> { }

        public class AddSessionRequest
        {
            public SessionInfo Session { get; }

            public LogFactory LogFactory { get; }


            public AddSessionRequest(SessionInfo session, LogFactory logFactory)
            {
                Session = session;
                LogFactory = logFactory;
            }
        }

        public class SessionInfoRequest
        {
            public string Id { get; }


            public SessionInfoRequest(string id)
            {
                Id = id;
            }
        }

        public class RemoveSessionCmd
        {
            public string Id { get; }


            public RemoveSessionCmd(string id)
            {
                Id = id;
            }
        }

        public class OpenUpdatesChannelCmd
        {
            public string SessionId { get; }

            public IServerStreamWriter<UpdateInfo> NetworkStream { get; }


            public OpenUpdatesChannelCmd(string sessionId, IServerStreamWriter<UpdateInfo> networkStream)
            {
                SessionId = sessionId;
                NetworkStream = networkStream;
            }
        }

        public class AddPluginLogsSubRequest
        {
            public string SessionId { get; }

            public string PluginId { get; }

            public AddPluginLogsSubRequest(string sessionId, string pluginId)
            {
                SessionId = sessionId;
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
            public string SessionId { get; }

            public string PluginId { get; }

            public AddPluginStatusSubRequest(string sessionId, string pluginId)
            {
                SessionId = sessionId;
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
