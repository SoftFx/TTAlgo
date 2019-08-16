using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TickTrader.Algo.Common.Lib;

namespace TickTrader.BotTerminal
{
    [DataContract(Namespace = "")]
    internal class AuthStorageModel : StorageModelBase<AuthStorageModel>
    {
        [DataMember(Name = "Accounts")]
        private VarList<AccountStorageEntry> _accounts;

        [DataMember]
        public string LastLogin { get; set; }

        [DataMember]
        public string LastServer { get; set; }

        public VarList<AccountStorageEntry> Accounts => _accounts;

        public AuthStorageModel()
        {
            _accounts = new VarList<AccountStorageEntry>();
        }

        public override AuthStorageModel Clone()
        {
            return new AuthStorageModel()
            {
                LastLogin = LastLogin,
                LastServer = LastServer,
                _accounts = new VarList<AccountStorageEntry>(_accounts.Values.Select(a => a.Clone())),
            };
        }

        public void UpdateLast(string login, string server)
        {
            LastLogin = login;
            LastServer = server;
        }

        public void Remove(AccountStorageEntry account)
        {
            var index = _accounts.Values.IndexOf(a => a.Login == account.Login && a.ServerAddress == account.ServerAddress);
            if (index != -1)
                _accounts.RemoveAt(index);
        }

        public void Update(AccountStorageEntry account)
        {
            int index = _accounts.Values.IndexOf(a => a.Login == account.Login && a.ServerAddress == account.ServerAddress);
            if (index < 0)
                _accounts.Values.Add(account);
            else
            {
                var toUpdate = _accounts[index];
                toUpdate.Password = account.Password;
            }
        }
    }
}
