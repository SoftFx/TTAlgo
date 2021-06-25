using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Server
{
    public class AccountConsumerControllerModel
    {
        private readonly IActorRef _ref;


        public AccountConsumerControllerModel(IActorRef actor)
        {
            _ref = actor;
        }


        public Task AttachSession(RpcSession session) => _ref.Ask(new AccountConsumerController.AttachSessionCmd(session));

        public Task DetachSession(string sessionId) => _ref.Ask(new AccountConsumerController.DetachSessionCmd(sessionId));

        public Task<IAccountProxy> GetAccountProxy() => _ref.Ask<IAccountProxy>(new AccountConsumerController.AccountProxyRequest());
    }
}
