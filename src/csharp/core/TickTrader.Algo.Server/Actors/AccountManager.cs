using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Account;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Server.Persistence;

namespace TickTrader.Algo.Server
{
    internal class AccountManager : Actor
    {
        private const int AccountShutdownTimeout = 5000;

        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<AccountManager>();

        private readonly AlgoServer _server;
        private readonly Dictionary<string, AccountConsumerControllerModel> _accountsMap2 = new Dictionary<string, AccountConsumerControllerModel>();
        private readonly Dictionary<string, AccountControlModel> _accounts = new Dictionary<string, AccountControlModel>();
        private readonly Dictionary<string, string> _accountDisplayNameCache = new Dictionary<string, string>();


        private AccountManager(AlgoServer server)
        {
            _server = server;

            Receive<RegisterAccountProxyCmd, object>(RegisterAccountProxy);
            Receive<ConsumerControllerRequest, AccountConsumerControllerModel>(GetConsumerController);
            Receive<ShutdownCmd>(Shutdown);
            Receive<RestoreCmd>(Restore);
            Receive<AddAccountRequest>(AddAccount);
            Receive<ChangeAccountRequest>(ChangeAccount);
            Receive<RemoveAccountRequest>(RemoveAccount);
            Receive<TestAccountRequest, ConnectionErrorInfo>(TestAccount);
            Receive<TestAccountCredsRequest, ConnectionErrorInfo>(TestAccountCreds);
            Receive<AccountMetadataRequest, AccountMetadataInfo>(GetMetadata);
        }


        public static IActorRef Create(AlgoServer server)
        {
            return ActorSystem.SpawnLocal(() => new AccountManager(server), nameof(AccountManager));
        }


        private object RegisterAccountProxy(RegisterAccountProxyCmd cmd)
        {
            var accProxy = cmd.Account;
            var accId = accProxy.Id;
            if (_accountsMap2.ContainsKey(accId))
                return Errors.DuplicateAccount(accId);

            var acc = new AccountConsumerControllerModel(AccountConsumerController.Create(accProxy));
            _accountsMap2.Add(accId, acc);

            return null;
        }

        private AccountConsumerControllerModel GetConsumerController(ConsumerControllerRequest request)
        {
            var accId = request.AccountId;
            if (!_accountsMap2.TryGetValue(accId, out var account))
                throw Errors.AccountNotFound(accId);

            return account;
        }

        private async Task Shutdown(ShutdownCmd cmd)
        {
            _logger.Debug("Stopping...");

            await Task.WhenAll(_accounts.Select(p => ShutdownAccountInternal(p.Key, p.Value)));

            _logger.Debug("Stopped");
        }

        private async Task Restore(RestoreCmd cmd)
        {
            _logger.Debug("Restoring saved state...");

            var savedState = await _server.SavedState.GetSnapshot();
            foreach(var acc in savedState.Accounts.Values)
            {
                CreateAccountInternal(acc);
            }

            _logger.Debug("Restored saved state");
        }

        private async Task AddAccount(AddAccountRequest request)
        {
            var server = request.Server;
            var userId = request.UserId;
            var creds = request.Creds;

            Validate(server, userId, creds);

            var accId = AccountId.Pack(server, userId);
            if (_accounts.ContainsKey(accId))
                throw Errors.DuplicateAccount(accId);

            var displayName = request.DisplayName;
            if (string.IsNullOrEmpty(displayName))
                displayName = $"{server} - {userId}";

            if (_accountDisplayNameCache.Values.Any(name => name == displayName))
                throw Errors.DuplicateAccountDisplayName(displayName, server);

            var savedState = new AccountSavedState
            {
                Id = accId,
                Server = server,
                UserId = userId,
                DisplayName = displayName,
            };
            savedState.PackCreds(request.Creds);

            await _server.SavedState.AddAccount(savedState);

            CreateAccountInternal(savedState);
        }

        private async Task ChangeAccount(ChangeAccountRequest request)
        {
            var accId = request.AccountId;
            if (!_accounts.TryGetValue(accId, out var account))
                throw Errors.AccountNotFound(accId);

            await account.Change(request);

            var displayName = request.DisplayName;
            if (!string.IsNullOrEmpty(displayName))
                _accountDisplayNameCache[accId] = displayName;
        }

        private async Task RemoveAccount(RemoveAccountRequest request)
        {
            var accId = request.AccountId;
            if (!_accounts.TryGetValue(accId, out var account))
                throw Errors.AccountNotFound(accId);

            await _server.SavedState.RemoveAccount(accId);

            _accounts.Remove(accId);
            _accountDisplayNameCache.Remove(accId);

            _server.EventBus.SendUpdate(AccountModelUpdate.Removed(accId));

            await ShutdownAccountInternal(accId, account);
        }

        private Task<ConnectionErrorInfo> TestAccount(TestAccountRequest request)
        {
            var accId = request.AccountId;
            if (!_accounts.TryGetValue(accId, out var account))
                throw Errors.AccountNotFound(accId);

            return account.Test(request);
        }

        private async Task<ConnectionErrorInfo> TestAccountCreds(TestAccountCredsRequest request)
        {
            var server = request.Server;
            var userId = request.UserId;
            var creds = request.Creds;

            Validate(server, userId, creds);

            try
            {
                var options = new ConnectionOptions { EnableLogs = false, LogsFolder = _server.Env.LogFolder, Type = AppType.BotAgent };
                var client = new ClientModel.ControlHandler2(KnownAccountFactories.Fdk2, options,
                        _server.Env.FeedHistoryCacheFolder, FeedHistoryFolderOptions.ServerClientHierarchy, "test" + Guid.NewGuid().ToString("N"));

                await client.OpenHandler();

                var lastError = await client.Connection.Connect(userId, creds.GetPassword(), server, CancellationToken.None);

                await client.Connection.Disconnect();

                await client.CloseHandler();

                return lastError;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to test account creds");
            }

            return ConnectionErrorInfo.UnknownNoText;
        }

        private Task<AccountMetadataInfo> GetMetadata(AccountMetadataRequest request)
        {
            var accId = request.AccountId;
            if (!_accounts.TryGetValue(accId, out var account))
                throw Errors.AccountNotFound(accId);

            return account.GetMetadata(request);
        }


        private void CreateAccountInternal(AccountSavedState savedState)
        {
            var accId = savedState.Id;
            var acc = new AccountControlModel(AccountControlActor.Create(_server, savedState));
            _accounts[accId] = acc;
            _accountDisplayNameCache[accId] = savedState.DisplayName;
        }
        
        private async Task ShutdownAccountInternal(string accId, AccountControlModel acc)
        {
            try
            {
                if (!await acc.Shutdown().WaitAsync(AccountShutdownTimeout))
                    _logger.Error($"Failed to shutdown account '{accId}': Timeout");
            }
            catch(Exception ex)
            {
                _logger.Error(ex, $"Failed to shutdown account '{accId}'");
            }
        }

        private void Validate(string server, string userId, AccountCreds creds)
        {
            if (string.IsNullOrWhiteSpace(server))
                throw new AlgoException("Server is required");
            if (string.IsNullOrWhiteSpace(userId))
                throw new AlgoException("UserId is required");
            switch (creds.AuthScheme)
            {
                case AccountCreds.SimpleAuthSchemeId:
                    if (string.IsNullOrWhiteSpace(creds.GetPassword()))
                        throw new AlgoException("Password is required");
                    break;
                default:
                    throw new AlgoException("Creds.AuthScheme is not supported");
            }
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

        internal class ShutdownCmd : Singleton<ShutdownCmd> { }

        internal class RestoreCmd : Singleton<RestoreCmd> { }
    }
}
