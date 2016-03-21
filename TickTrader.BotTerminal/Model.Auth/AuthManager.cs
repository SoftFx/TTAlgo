using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TickTrader.BotTerminal
{
    internal class AuthManager
    {
        private AuthStorageModel storage;

        public AuthManager(AuthStorageModel storage)
        {
            this.storage = storage;
            this.storage.Accounts.Changed += Accounts_Changed;

            Accounts = new ObservableCollection<AccountAuthEntry>();
            Servers = new ObservableCollection<ServerAuthEntry>();
            Init();
        }

        public void SaveLogin(string login, string server)
        {
            SaveLogin(login, null, server);
        }

        public void SaveLogin(string login, string password, string server)
        {
            storage.Update(new AccountSorageEntry(login, password, server));
        }

        private void Accounts_Changed(object sender, ListChangedEventArgs<AccountSorageEntry> e)
        {
            if (e.Action == CollectionChangeActions.Added)
                Accounts.Add(CreateNewEntry(e.NewItem));
            else if (e.Action == CollectionChangeActions.Removed)
            {
                var index = Accounts.IndexOf(a => a.Matches(e.OldItem));
                Accounts.RemoveAt(index);
            }
            else if (e.Action == CollectionChangeActions.Replaced)
            {
                var index = Accounts.IndexOf(a => a.Matches(e.OldItem));
                Accounts[index] = CreateNewEntry(e.NewItem);
            }
        }

        private void Init()
        {
            AuthConfigSection cfgSection = AuthConfigSection.GetCfgSection();

            foreach (ServerElement predefinedServer in cfgSection.Servers)
                Servers.Add(new ServerAuthEntry(predefinedServer));

            foreach (var acc in storage.Accounts)
                Accounts.Add(CreateNewEntry(acc));
        }

        private AccountAuthEntry CreateNewEntry(AccountSorageEntry record)
        {
            return new AccountAuthEntry(record, GetServer(record.ServerAddress));
        }

        private ServerAuthEntry GetServer(string address)
        {
            var server = Servers.FirstOrDefault(s => s.Address == address);

            if (server != null)
                return server;

            return new ServerAuthEntry(address);
        }

        public ObservableCollection<AccountAuthEntry> Accounts { get; private set; }
        public ObservableCollection<ServerAuthEntry> Servers { get; private set; }
    }

    internal class ServerAuthEntry
    {
        public ServerAuthEntry(ServerElement cfgElement)
            : this(cfgElement.Name, cfgElement.ShortName, cfgElement.Address, cfgElement.Color)
        {
        }

        public ServerAuthEntry(string address)
            : this(address, address, address, null)
        {
        }

        public ServerAuthEntry(string name, string shortName, string address, string color)
        {
            this.Name = name;
            this.Address = address;
            this.ShortName = shortName;

            if (string.IsNullOrWhiteSpace(this.ShortName))
                this.ShortName = name;

            if (!string.IsNullOrWhiteSpace(color))
                this.Color = (Color)ColorConverter.ConvertFromString(color);

            if (string.IsNullOrWhiteSpace(name))
                this.Name = address;
        }

        public string Name { get; }
        public string ShortName { get; }
        public string Address { get; }
        public Color Color { get; }
    }

    internal class AccountAuthEntry
    {
        private AccountSorageEntry storageRecord;

        public AccountAuthEntry(AccountSorageEntry storageRecord, ServerAuthEntry server)
        {
            this.storageRecord = storageRecord;
            this.Server = server;
        }

        public ServerAuthEntry Server { get; private set; }
        public string Login { get { return storageRecord.Login; } }

        public bool Matches(AccountSorageEntry acc)
        {
            return Login == acc.Login && Server.Address == acc.ServerAddress;
        }

        public string Password
        {
            get { return storageRecord.Password; }
            set { storageRecord.Password = value; }
        }
    }
}
