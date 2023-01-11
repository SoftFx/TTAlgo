using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Server
{
    public class AccountRpcModel
    {
        public static Task<AccountRpcHandler> AttachSession(IActorRef actor, RpcSession session) => actor.Ask<AccountRpcHandler>(new AttachSessionCmd(session));

        public static Task DetachSession(IActorRef actor, string sessionId) => actor.Ask(new DetachSessionCmd(sessionId));


        public record AttachSessionCmd(RpcSession Session);

        public record DetachSessionCmd(string SessionId);
    }
}
