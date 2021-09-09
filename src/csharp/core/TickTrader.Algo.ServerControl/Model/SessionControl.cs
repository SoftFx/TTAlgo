using NLog;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.Algo.ServerControl.Model
{
    internal static class SessionControl
    {
        public static Task<SessionInfo> CreateSession(IActorRef actor, string userId, int minorVersion, ClientClaims.Types.AccessLevel accessLevel, LogFactory logFactory)
            => actor.Ask<SessionInfo>(new CreateSessionRequest(userId, minorVersion, accessLevel, logFactory));

        public static Task<SessionInfo> GetSession(IActorRef actor, string sessionId) => actor.Ask<SessionInfo>(new SessionInfoRequest(sessionId));


        public class CreateSessionRequest
        {
            public string UserId { get; }

            public int MinorVersion { get; }

            public ClientClaims.Types.AccessLevel AccessLevel { get; }

            public LogFactory LogFactory { get; }


            public CreateSessionRequest(string userId, int minorVersion, ClientClaims.Types.AccessLevel accessLevel, LogFactory logFactory)
            {
                UserId = userId;
                MinorVersion = minorVersion;
                AccessLevel = accessLevel;
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
    }
}
