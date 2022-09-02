using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Server
{
    public class AccountControlModel
    {
        public static Task Shutdown(IActorRef actor) => actor.Ask(AccountControlActor.ShutdownCmd.Instance);

        public static Task Change(IActorRef actor, ChangeAccountRequest request) => actor.Ask(request);

        public static Task<AccountMetadataInfo> GetMetadata(IActorRef actor, AccountMetadataRequest request) => actor.Ask<AccountMetadataInfo>(request);

        public static Task<ConnectionErrorInfo> Test(IActorRef actor, TestAccountRequest request) => actor.Ask<ConnectionErrorInfo>(request);

        public static Task<AccountRpcHandler> AttachSession(IActorRef actor, RpcSession session) => actor.Ask<AccountRpcHandler>(new AccountRpcController.AttachSessionCmd(session));

        public static Task DetachSession(IActorRef actor, string sessionId) => actor.Ask(new AccountRpcController.DetachSessionCmd(sessionId));
    }
}
