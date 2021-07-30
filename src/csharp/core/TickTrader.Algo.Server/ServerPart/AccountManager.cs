using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Account;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Server.Persistence;

namespace TickTrader.Algo.Server
{
    internal class AccountManager
    {
        private const int AccountShutdownTimeout = 5000;

        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<AccountManager>();

        private readonly AlgoServerPrivate _server;
        private readonly Dictionary<string, IActorRef> _accounts = new Dictionary<string, IActorRef>();
        private readonly Dictionary<string, string> _accountDisplayNameCache = new Dictionary<string, string>();


        public AccountManager(AlgoServerPrivate server)
        {
            _server = server;
        }


        public async Task Shutdown()
        {
            _logger.Debug("Stopping...");

            await Task.WhenAll(_accounts.Select(p => ShutdownAccountInternal(p.Key, p.Value)));

            _logger.Debug("Stopped");
        }

        public void Restore(ServerSavedState savedState)
        {
            _logger.Debug("Restoring saved state...");

            foreach (var acc in savedState.Accounts.Values)
            {
                CreateAccountInternal(acc);
            }

            _logger.Debug("Restored saved state");
        }

        public Task<AccountControlModel> GetAccountControl(string accId)
        {
            if (!_accounts.TryGetValue(accId, out var account))
                return Task.FromException<AccountControlModel>(Errors.AccountNotFound(accId));

            return Task.FromResult(new AccountControlModel(account));
        }

        public async Task AddAccount(AddAccountRequest request)
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

        public async Task ChangeAccount(ChangeAccountRequest request)
        {
            var accId = request.AccountId;
            if (!_accounts.TryGetValue(accId, out var account))
                throw Errors.AccountNotFound(accId);

            await account.Ask(request);

            var displayName = request.DisplayName;
            if (!string.IsNullOrEmpty(displayName))
                _accountDisplayNameCache[accId] = displayName;
        }

        public async Task RemoveAccount(RemoveAccountRequest request)
        {
            var accId = request.AccountId;
            if (!_accounts.TryGetValue(accId, out var account))
                throw Errors.AccountNotFound(accId);

            await _server.SavedState.RemoveAccount(accId);

            _accounts.Remove(accId);
            _accountDisplayNameCache.Remove(accId);

            try
            {
                await ShutdownAccountInternal(accId, account);
            }
            finally
            {
                _server.SendUpdate(AccountModelUpdate.Removed(accId));
            }
        }

        public Task<ConnectionErrorInfo> TestAccount(TestAccountRequest request)
        {
            var accId = request.AccountId;
            if (!_accounts.TryGetValue(accId, out var account))
                throw Errors.AccountNotFound(accId);

            return account.Ask<ConnectionErrorInfo>(request);
        }

        public async Task<ConnectionErrorInfo> TestAccountCreds(TestAccountCredsRequest request)
        {
            var server = request.Server;
            var userId = request.UserId;
            var creds = request.Creds;

            Validate(server, userId, creds);

            try
            {
                var client = new ClientModel.ControlHandler2(KnownAccountFactories.Fdk2, _server.AccountOptions,
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

        public Task<AccountMetadataInfo> GetMetadata(AccountMetadataRequest request)
        {
            var accId = request.AccountId;
            if (!_accounts.TryGetValue(accId, out var account))
                throw Errors.AccountNotFound(accId);

            return account.Ask<AccountMetadataInfo>(request);
        }


        private void CreateAccountInternal(AccountSavedState savedState)
        {
            var accId = savedState.Id;
            var acc = AccountControlActor.Create(_server, savedState);
            _accounts[accId] = acc;
            _accountDisplayNameCache[accId] = savedState.DisplayName;
        }

        private async Task ShutdownAccountInternal(string accId, IActorRef acc)
        {
            try
            {
                if (!await acc.Ask(AccountControlActor.ShutdownCmd.Instance).WaitAsync(AccountShutdownTimeout))
                    _logger.Error($"Failed to shutdown account '{accId}': Timeout");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to shutdown account '{accId}'");
            }

            await ActorSystem.StopActor(acc)
                    .OnException(ex => _logger.Error(ex, $"Failed to stop actor {acc.Name}"));
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
    }
}
