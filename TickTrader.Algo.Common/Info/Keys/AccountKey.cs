namespace TickTrader.Algo.Common.Info
{
    public class AccountKey
    {
        public string Server { get; set; }

        public string Login { get; set; }


        public AccountKey()
        {
        }

        public AccountKey(string server, string login)
        {
            Server = server;
            Login = login;
        }


        public override string ToString()
        {
            return $"Account {Login} at {Server}";
        }

        public override int GetHashCode()
        {
            return $"{Server}{Login}".GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var key = obj as AccountKey;
            return key != null
                && key.Login == Login
                && key.Server == Server;
        }
    }
}
