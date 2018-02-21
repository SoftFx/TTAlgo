using Machinarium.Qnil;
using System;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Protocol;

namespace TickTrader.BotTerminal
{
    internal class BotAgentModel : IBotAgentClient
    {
        private ISyncContext _syncContext;
        private DynamicDictionary<string, AccountModelEntity> _accounts;
        private DynamicDictionary<string, BotModelEntity> _bots;
        private DynamicDictionary<string, PackageModelEntity> _packages;


        public IDynamicDictionarySource<string, AccountModelEntity> Accounts => _accounts;

        public IDynamicDictionarySource<string, BotModelEntity> Bots => _bots;

        public IDynamicDictionarySource<string, PackageModelEntity> Packages => _packages;


        public Action<string> BotStateChanged = delegate { };

        public Action<string> AccountStateChanged = delegate { };


        public BotAgentModel()
        {
            _syncContext = new DispatcherSync();

            _accounts = new DynamicDictionary<string, AccountModelEntity>();
            _bots = new DynamicDictionary<string, BotModelEntity>();
            _packages = new DynamicDictionary<string, PackageModelEntity>();
        }


        public static string GetAccountKey(AccountModelEntity account)
        {
            return GetAccountKey(account.Server, account.Login);
        }

        public static string GetAccountKey(AccountKeyEntity accountKey)
        {
            return GetAccountKey(accountKey.Server, accountKey.Login);
        }

        public static string GetAccountKey(string server, string login)
        {
            return $"{server} - {login}";
        }


        public void ClearCache()
        {
            _syncContext.Invoke(() =>
            {
                _accounts.Clear();
                _bots.Clear();
                _packages.Clear();
            });
        }


        #region IBotAgentClient implementation

        public void InitAccountList(AccountListReportEntity report)
        {
            _syncContext.Invoke(() =>
            {
                _accounts.Clear();
                foreach (var acc in report.Accounts)
                {
                    _accounts.Add(GetAccountKey(acc), acc);
                }
            });
        }

        public void InitBotList(BotListReportEntity report)
        {
            _syncContext.Invoke(() =>
            {
                _bots.Clear();
                foreach (var bot in report.Bots)
                {
                    _bots.Add(bot.InstanceId, bot);
                }
            });
        }

        public void InitPackageList(PackageListReportEntity report)
        {
            _syncContext.Invoke(() =>
            {
                _packages.Clear();
                foreach (var package in report.Packages)
                {
                    _packages.Add(package.Name, package);
                }
            });
        }

        public void UpdateAccount(AccountModelUpdateEntity update)
        {
            _syncContext.Invoke(() =>
            {
                var acc = update.Item;
                var key = GetAccountKey(acc);
                switch (update.Type)
                {
                    case UpdateType.Added:
                    case UpdateType.Updated:
                        _accounts[key] = acc;
                        break;
                    case UpdateType.Removed:
                        if (_accounts.ContainsKey(key))
                            _accounts.Remove(key);
                        break;
                }
            });
        }

        public void UpdateBot(BotModelUpdateEntity update)
        {
            _syncContext.Invoke(() =>
            {
                var bot = update.Item;
                switch (update.Type)
                {
                    case UpdateType.Added:
                    case UpdateType.Updated:
                        _bots[bot.InstanceId] = bot;
                        break;
                    case UpdateType.Removed:
                        if (_bots.ContainsKey(bot.InstanceId))
                            _bots.Remove(bot.InstanceId);
                        break;
                }
            });
        }

        public void UpdatePackage(PackageModelUpdateEntity update)
        {
            _syncContext.Invoke(() =>
            {
                var package = update.Item;
                switch (update.Type)
                {
                    case UpdateType.Added:
                    case UpdateType.Updated:
                        _packages[package.Name] = package;
                        break;
                    case UpdateType.Removed:
                        if (_packages.ContainsKey(package.Name))
                            _packages.Remove(package.Name);
                        break;
                }
            });
        }

        public void UpdateBotState(BotStateUpdateEntity update)
        {
            _syncContext.Invoke(() =>
            {
                if (_bots.ContainsKey(update.BotId))
                {
                    _bots[update.BotId].State = update.State;
                    BotStateChanged(update.BotId);
                }
            });
        }

        public void UpdateAccountState(AccountStateUpdateEntity update)
        {
            _syncContext.Invoke(() =>
            {
                var key = GetAccountKey(update.Account);
                if (_accounts.ContainsKey(key))
                {
                    _accounts[key].ConnectionState = update.ConnectionState;
                    _accounts[key].LastError = update.LastError;
                    AccountStateChanged(key);
                }
            });
        }

        #endregion
    }
}
