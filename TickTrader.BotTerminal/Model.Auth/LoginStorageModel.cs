using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    [Serializable]
    public class LoginStorageModel : IPersistableObject
    {
        private ObservableList<ServerStorageEntry> servers = new ObservableList<ServerStorageEntry>();
        private ObservableList<AccountSorageEntry> accounts = new ObservableList<AccountSorageEntry>();

        public LoginStorageModel()
        {
            accounts.Changed += (s, a) => OnChanged();
            servers.Changed += (s, a) => OnChanged();
        }

        public LoginStorageModel(LoginStorageModel src)
        {
        }

        public IReadonlyObservableList<ServerStorageEntry> Servers { get { return servers; } }
        public IReadonlyObservableList<AccountSorageEntry> Accounts { get { return accounts; } }

        public void Add(AccountSorageEntry account)
        {
            accounts.Add(account);
        }

        public void Remove(AccountSorageEntry account)
        {
            accounts.Remove(account);
        }

        public void Update(AccountSorageEntry account)
        {
            int index = accounts.IndexOf(a => a.Login == account.Login && a.ServerId == account.ServerId);
        }

        public event Action Changed;

        public object Clone()
        {
            return new LoginStorageModel(this);
        }

        private void OnChanged()
        {
            if (this.Changed != null)
                Changed();
        }
    }

    [Serializable]
    public class AccountSorageEntry
    {
        public AccountSorageEntry(string login, string password, string serverId)
        {
            this.Login = login;
            this.Password = password;
            this.ServerId = serverId;
        }

        public string Login { get; private set; }
        public string Password { get; private set; }
        public string ServerId { get; private set; }

        public AccountSorageEntry Clone()
        {
            return new AccountSorageEntry(Login, Password, ServerId);
        }
    }

    [Serializable]
    public class ServerStorageEntry
    {
        public ServerStorageEntry(string name, string address, ServerAddressTypes type)
        {
            this.Name = name;
            this.Address = address;
            this.Type = type;
        }

        public string Name { get; private set; }
        public string Address { get; private set; }
        public ServerAddressTypes Type { get; private set; }

        public ServerStorageEntry Clone()
        {
            return new ServerStorageEntry(Name, Address, Type);
        }
    }

    public enum ServerAddressTypes { Live, Demo, Custom }
}
