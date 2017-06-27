using Machinarium.Qnil;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace TickTrader.BotTerminal
{
    [DataContract(Namespace = "")]
    internal class AuthStorageModel : StorageModelBase<AuthStorageModel>
    {
        [DataMember(Name = "Accounts")]
        private DynamicList<AccountSorageEntry> _accounts;


        [DataMember]
        public string LastLogin { get; set; }

        [DataMember]
        public string LastServer { get; set; }

        public DynamicList<AccountSorageEntry> Accounts => _accounts;


        public AuthStorageModel()
        {
            _accounts = new DynamicList<AccountSorageEntry>();
        }


        public override AuthStorageModel Clone()
        {
            return new AuthStorageModel()
            {
                LastLogin = LastLogin,
                LastServer = LastServer,
                _accounts = new DynamicList<AccountSorageEntry>(_accounts.Values.Select(a => a.Clone())),
            };
        }


        public void UpdateLast(string login, string server)
        {
            LastLogin = login;
            LastServer = server;
        }

        public void Remove(AccountSorageEntry account)
        {
            var index = _accounts.Values.IndexOf(a => a.Login == account.Login && a.ServerAddress == account.ServerAddress);
            if (index != -1)
                _accounts.RemoveAt(index);
        }

        public void Update(AccountSorageEntry account)
        {
            int index = _accounts.Values.IndexOf(a => a.Login == account.Login && a.ServerAddress == account.ServerAddress);
            if (index < 0)
                _accounts.Values.Add(account);
            else
            {
                if (_accounts.Values[index].Password != account.Password)
                    _accounts.Values[index].Password = account.Password;
            }
        }
    }
}
