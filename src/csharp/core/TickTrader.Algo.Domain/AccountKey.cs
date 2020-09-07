using System;

namespace TickTrader.Algo.Domain
{
    public partial class AccountKey : IComparable<AccountKey>
    {
        public AccountKey(string server, string login)
        {
            Server = server;
            Login = login;
        }


        public int CompareTo(AccountKey other)
        {
            var res = Server.CompareTo(other.Server);
            if (res == 0)
                return Login.CompareTo(other.Login);
            return res;
        }
    }
}
