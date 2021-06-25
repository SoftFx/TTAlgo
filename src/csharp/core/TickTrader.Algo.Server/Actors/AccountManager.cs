using System.Collections.Generic;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Server
{
    internal class AccountManager : Actor
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<AccountManager>();

        private readonly AlgoServer _server;
        private readonly Dictionary<string, AccountConsumerControllerModel> _accountsMap = new Dictionary<string, AccountConsumerControllerModel>();


        private AccountManager(AlgoServer server)
        {
            _server = server;

            Receive<RegisterAccountProxyCmd, object>(RegisterAccountProxy);
            Receive<ConsumerControllerRequest, AccountConsumerControllerModel>(GetConsumerController);
        }


        public static IActorRef Create(AlgoServer server)
        {
            return ActorSystem.SpawnLocal(() => new AccountManager(server), nameof(AccountManager));
        }


        private object RegisterAccountProxy(RegisterAccountProxyCmd cmd)
        {
            var accProxy = cmd.Account;
            var accId = accProxy.Id;
            if (_accountsMap.ContainsKey(accId))
                return Errors.DuplicateAccount(accId);

            var acc = new AccountConsumerControllerModel(AccountConsumerController.Create(accProxy));
            _accountsMap.Add(accId, acc);

            return null;
        }

        private AccountConsumerControllerModel GetConsumerController(ConsumerControllerRequest request)
        {
            var accId = request.AccountId;
            if (_accountsMap.TryGetValue(accId, out var account))
                throw Errors.AccountNotFound(accId);

            return account;
        }


        internal class RegisterAccountProxyCmd
        {
            public IAccountProxy Account { get; }

            public RegisterAccountProxyCmd(IAccountProxy account)
            {
                Account = account;
            }
        }

        internal class ConsumerControllerRequest
        {
            public string AccountId { get; }

            public ConsumerControllerRequest(string accountId)
            {
                AccountId = accountId;
            }
        }
    }
}
