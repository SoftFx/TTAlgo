using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TickTrader.BotTerminal
{
    [Serializable]
    public class AuthStorageModel : IChangableObject, IPersistableObject<AuthStorageModel>
    {
        private ObservableList<AccountSorageEntry> accounts;
        private string lastLogin;
        private string lastServer;

        public AuthStorageModel()
        {
            accounts = new ObservableList<AccountSorageEntry>();
        }

        public string LastLogin
        {
            get { return lastLogin; }
            set { lastLogin = value; }
        }

        public string LastServer
        {
            get { return lastServer; }
            set { lastServer = value; }
        }

        public void UpdateLast(string login, string server)
        {
            lastLogin = login;
            lastServer = server;
        }

        public void TriggerSave()
        {
            OnChanged();
        }

        public AuthStorageModel(AuthStorageModel src)
        {
            accounts = new ObservableList<AccountSorageEntry>(src.accounts.Select(a => a.Clone()));
            lastLogin = src.lastLogin;
            lastServer = src.lastServer;
        }

        public ObservableList<AccountSorageEntry> Accounts { get { return accounts; } }

        public void Remove(AccountSorageEntry account)
        {
            var index = accounts.IndexOf(a => a.Login == account.Login && a.ServerAddress == account.ServerAddress);
            if (index != -1)
                accounts.RemoveAt(index);
        }

        public void Update(AccountSorageEntry account)
        {
            int index = accounts.IndexOf(a => a.Login == account.Login && a.ServerAddress == account.ServerAddress);
            if (index < 0)
                accounts.Add(account);
            else
            {
                if (accounts[index].Password != account.Password)
                    accounts[index].Password = account.Password;
            }
        }

        public event Action Changed;

        private void OnChanged()
        {
            if (this.Changed != null)
                Changed();
        }

        public AuthStorageModel GetCopyToSave()
        {
            return new AuthStorageModel(this);
        }

        
    }

    [Serializable]
    public class AccountSorageEntry
    {
        private string password;
        private string login;
        private string server;

        public AccountSorageEntry()
        {
        }

        public AccountSorageEntry(string login, string password, string server)
        {
            this.login = login;
            this.password = password;
            this.server = server;
        }

        public string Login { get { return login; } set { login = value; } }
        public string ServerAddress { get { return server; } set { server = value; } }
        public bool HasPassword { get { return password != null; } }
 
        public string Password { get { return password; } set { password = value; } }

        public AccountSorageEntry Clone()
        {
            return new AccountSorageEntry(Login, Password, ServerAddress);
        }
    }
}
