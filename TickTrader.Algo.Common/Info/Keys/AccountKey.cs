using System;

namespace TickTrader.Algo.Common.Info
{
    //public class AccountKey : IComparable<AccountKey>
    //{
    //    public string Server { get; set; }

    //    public string Login { get; set; }


    //    public AccountKey()
    //    {
    //    }

    //    public AccountKey(string server, string login)
    //    {
    //        Server = server;
    //        Login = login;
    //    }


    //    public override string ToString()
    //    {
    //        return $"Account {Login} at {Server}";
    //    }

    //    public override int GetHashCode()
    //    {
    //        return $"{Server}{Login}".GetHashCode();
    //    }

    //    public override bool Equals(object obj)
    //    {
    //        var key = obj as AccountKey;
    //        return key != null
    //            && key.Login == Login
    //            && key.Server == Server;
    //    }

    //    public int CompareTo(AccountKey other)
    //    {
    //        var res = Server.CompareTo(other.Server);
    //        if (res == 0)
    //            return Login.CompareTo(other.Login);
    //        return res;
    //    }
    //}
}
