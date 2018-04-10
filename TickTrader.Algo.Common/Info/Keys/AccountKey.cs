namespace TickTrader.Algo.Common.Info
{
    public class AccountKey
    {
        public string Server { get; set; }

        public string Login { get; set; }


        public AccountKey() { }

        public AccountKey(string server, string login)
        {
            Server = server;
            Login = login;
        }


        public override string ToString()
        {
            return $"account {Login} at {Server}";
        }
    }
}
