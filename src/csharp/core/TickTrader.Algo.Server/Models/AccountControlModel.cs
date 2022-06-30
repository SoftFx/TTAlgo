using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Server
{
    public class AccountControlModel
    {
        private readonly IActorRef _ref;


        public AccountControlModel(IActorRef actor)
        {
            _ref = actor;
        }


        public Task Shutdown() => _ref.Ask(AccountControlActor.ShutdownCmd.Instance);

        public Task Change(ChangeAccountRequest request) => _ref.Ask(request);

        public Task<AccountMetadataInfo> GetMetadata(AccountMetadataRequest request) => _ref.Ask<AccountMetadataInfo>(request);

        public Task<ConnectionErrorInfo> Test(TestAccountRequest request) => _ref.Ask<ConnectionErrorInfo>(request);

        public Task<AccountRpcHandler> AttachSession(RpcSession session) => _ref.Ask<AccountRpcHandler>(new AccountControlActor.AttachSessionCmd(session));

        public Task DetachSession(string sessionId) => _ref.Ask(new AccountControlActor.DetachSessionCmd(sessionId));

        public Task<IAccountProxy> GetAccountProxy() => _ref.Ask<IAccountProxy>(AccountControlActor.AccountProxyRequest.Instance);
    }
}
