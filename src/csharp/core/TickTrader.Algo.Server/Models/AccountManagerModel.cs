using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Server
{
    public class AccountManagerModel
    {
        private readonly IActorRef _ref;


        public AccountManagerModel(IActorRef actor)
        {
            _ref = actor;
        }


        public Task RegisterAccountProxy(IAccountProxy account) => _ref.Ask(new AccountManager.RegisterAccountProxyCmd(account));

        internal Task<AccountConsumerControllerModel> GetConsumerController(string accountId)
            => _ref.Ask<AccountConsumerControllerModel>(new AccountManager.ConsumerControllerRequest(accountId));
    }
}
