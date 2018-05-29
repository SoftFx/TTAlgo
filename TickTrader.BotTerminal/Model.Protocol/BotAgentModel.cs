using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Protocol;

namespace TickTrader.BotTerminal
{
    internal class BotAgentModel : IBotAgentClient
    {
        private ISyncContext _syncContext;
        private VarDictionary<PackageKey, PackageInfo> _packages;
        private VarDictionary<AccountKey, AccountKey> _accounts;
        private VarDictionary<string, string> _bots;


        public IVarSet<PackageKey, PackageInfo> Packages => _packages;

        public IVarSet<AccountKey, AccountKey> Accounts => _accounts;

        public IVarSet<string, string> Bots => _bots;


        public Action<string> BotStateChanged = delegate { };

        public Action<string> AccountStateChanged = delegate { };


        public BotAgentModel()
        {
            _syncContext = new DispatcherSync();

            _packages = new VarDictionary<PackageKey, PackageInfo>();
            _accounts = new VarDictionary<AccountKey, AccountKey>();
            _bots = new VarDictionary<string, string>();
        }


        public static string GetAccountKey(AccountKey account)
        {
            return GetAccountKey(account.Server, account.Login);
        }

        //public static string GetAccountKey(AccountKeyEntity accountKey)
        //{
        //    return GetAccountKey(accountKey.Server, accountKey.Login);
        //}

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

        public void InitPackageList(List<PackageInfo> packages)
        {
            _syncContext.Invoke(() =>
            {
                _packages.Clear();
                foreach (var package in packages)
                {
                    _packages.Add(package.Key, package);
                }
            });
        }

        public void InitAccountList(List<AccountKey> accounts)
        {
            _syncContext.Invoke(() =>
            {
                _accounts.Clear();
                foreach (var acc in accounts)
                {
                    _accounts.Add(acc, acc);
                }
            });
        }

        public void InitBotList(List<string> bots)
        {
            _syncContext.Invoke(() =>
            {
                _bots.Clear();
                foreach (var bot in bots)
                {
                    _bots.Add(bot, bot);
                }
            });
        }

        public void UpdatePackage(UpdateInfo<PackageInfo> update)
        {
            _syncContext.Invoke(() =>
            {
                var package = update.Value;
                switch (update.Type)
                {
                    case UpdateType.Added:
                    case UpdateType.Replaced:
                        _packages[package.Key] = package;
                        break;
                    case UpdateType.Removed:
                        if (_packages.ContainsKey(package.Key))
                            _packages.Remove(package.Key);
                        break;
                }
            });
        }

        public void UpdateAccount(UpdateInfo<AccountKey> update)
        {
            _syncContext.Invoke(() =>
            {
                var acc = update.Value;
                switch (update.Type)
                {
                    case UpdateType.Added:
                    case UpdateType.Replaced:
                        _accounts[acc] = acc;
                        break;
                    case UpdateType.Removed:
                        if (_accounts.ContainsKey(acc))
                            _accounts.Remove(acc);
                        break;
                }
            });
        }

        public void UpdateBot(UpdateInfo<string> update)
        {
            _syncContext.Invoke(() =>
            {
                var bot = update.Value;
                switch (update.Type)
                {
                    case UpdateType.Added:
                    case UpdateType.Replaced:
                        _bots[bot] = bot;
                        break;
                    case UpdateType.Removed:
                        if (_bots.ContainsKey(bot))
                            _bots.Remove(bot);
                        break;
                }
            });
        }

        //public void UpdateBotState(BotStateUpdateEntity update)
        //{
        //    _syncContext.Invoke(() =>
        //    {
        //        if (_bots.ContainsKey(update.BotId))
        //        {
        //            _bots[update.BotId].State = update.State;
        //            BotStateChanged(update.BotId);
        //        }
        //    });
        //}

        //public void UpdateAccountState(AccountStateUpdateEntity update)
        //{
        //    _syncContext.Invoke(() =>
        //    {
        //        var key = GetAccountKey(update.Account);
        //        if (_accounts.ContainsKey(key))
        //        {
        //            _accounts[key].ConnectionState = update.ConnectionState;
        //            _accounts[key].LastError = update.LastError;
        //            AccountStateChanged(key);
        //        }
        //    });
        //}

        #endregion
    }
}
