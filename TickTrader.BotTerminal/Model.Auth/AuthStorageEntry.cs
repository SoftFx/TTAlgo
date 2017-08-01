using System;
using System.Runtime.Serialization;

namespace TickTrader.BotTerminal
{
    [DataContract(Namespace = "", Name = "AccountSorageEntry")] // Removed typo from type name. This will keep existing account info deserializable.
    public class AccountStorageEntry
    {
        [DataMember]
        public string Login { get; set; }

        [DataMember]
        public string ServerAddress { get; set; }

        public bool HasPassword => Password != null;

        [DataMember]
        public string Password { get; set; }


        public AccountStorageEntry()
        {
        }

        public AccountStorageEntry(string login, string password, string server)
        {
            Login = login;
            Password = password;
            ServerAddress = server;
        }


        public AccountStorageEntry Clone()
        {
            return new AccountStorageEntry(Login, Password, ServerAddress);
        }
    }
}
