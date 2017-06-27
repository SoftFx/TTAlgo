using System;
using System.Runtime.Serialization;

namespace TickTrader.BotTerminal
{
    [DataContract(Namespace = "")]
    public class AccountSorageEntry // removing this typo will cause deserialization failure (no accounts will be loaded)
    {
        [DataMember]
        public string Login { get; set; }

        [DataMember]
        public string ServerAddress { get; set; }

        public bool HasPassword => Password != null;

        [DataMember]
        public string Password { get; set; }


        public AccountSorageEntry()
        {
        }

        public AccountSorageEntry(string login, string password, string server)
        {
            Login = login;
            Password = password;
            ServerAddress = server;
        }


        public AccountSorageEntry Clone()
        {
            return new AccountSorageEntry(Login, Password, ServerAddress);
        }
    }
}
