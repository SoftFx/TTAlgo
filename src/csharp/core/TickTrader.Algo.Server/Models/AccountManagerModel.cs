using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain.ServerControl;

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

        public Task Shutdown() => _ref.Ask(AccountManager.ShutdownCmd.Instance);

        public Task Restore() => _ref.Ask(AccountManager.RestoreCmd.Instance);

        public Task Add(AddAccountRequest request) => _ref.Ask(request);

        public Task Change(ChangeAccountRequest request) => _ref.Ask(request);

        public Task Remove(RemoveAccountRequest request) => _ref.Ask(request);
    }
}
