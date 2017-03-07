using Machinarium.ActorModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TickTrader.DedicatedServer.DS.Models
{
    public class ServerModel : Actor
    {
        private List<AccountModel> accounts = new List<AccountModel>();

        public ServerModel()
        {
            SyncObj = new object();
        }

        public object SyncObj { get; private set; }

        public IEnumerable<AccountModel> Accounts => accounts;
        public IEnumerable<TradeBotModel> TradeBots => accounts.SelectMany(a => a.Bots);
        public event Action<ClientModel3, ChangeAction> AccountChanged;
        public event Action<TradeBotModel, ChangeAction> BotChanged;

        public void AddAccount(string login, string password, string server)
        {
            lock (SyncObj)
            {
                var existing = FindAccount(login, server);
                if (existing != null)
                    throw new Exception();
                else
                {
                    var newAcc = new AccountModel();
                    newAcc.Init(SyncObj);
                    newAcc.Connection.Change(server, login, password);
                    accounts.Add(newAcc);
                }
            }
        }

        private AccountModel FindAccount(string login, string server)
        {
            return Accounts.FirstOrDefault(a => a.Connection.Username == login && a.Connection.Address == server);
        }

        public void RemoveAccount(string login, string server)
        {
            var acc = FindAccount(login, server);
            if (acc == null)
                throw new Exception();
            accounts.Remove(acc);
        }
    }

    public enum ChangeAction { Added, Removed, Modified }
}
