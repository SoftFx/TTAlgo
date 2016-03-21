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

        public AuthStorageModel()
        {
            accounts = new ObservableList<AccountSorageEntry>();

            accounts.Changed += (s, a) =>
            {
                if (a.HasNewItem)
                    a.NewItem.Changed += Account_Changed;
                if (a.HasOldItem)
                    a.OldItem.Changed -= Account_Changed;
                OnChanged();
            };
        }

        public AuthStorageModel(AuthStorageModel src)
        {
            accounts = new ObservableList<AccountSorageEntry>(src.accounts.Select(a => a.Clone()));
        }

        public ObservableList<AccountSorageEntry> Accounts { get { return accounts; } }

        public void Update(AccountSorageEntry account)
        {
            int index = accounts.IndexOf(a => a.Login == account.Login && a.ServerAddress == account.ServerAddress);
            if (index < 0)
                accounts.Add(account);
            else
            {
                if (accounts[index].Password != account.Password)
                    accounts[index] = account;
            }
        }

        public event Action Changed;

        private void OnChanged()
        {
            if (this.Changed != null)
                Changed();
        }

        private void Account_Changed()
        {
            OnChanged();
        }

        public AuthStorageModel GetCopyToSave()
        {
            return new AuthStorageModel(this);
        }
    }

    [Serializable]
    public class AccountSorageEntry : IChangableObject
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
 
        public string Password
        {
            get { return password; }
            set
            {
                password = value;
                if (Changed != null)
                    Changed();
            }
        }

        public event Action Changed;

        public AccountSorageEntry Clone()
        {
            return new AccountSorageEntry(Login, Password, ServerAddress);
        }
    }
}
