namespace TickTrader.Algo.Domain.ServerControl
{
    public partial class TestAccountCredsRequest
    {
        public TestAccountCredsRequest(string server, string userId, AccountCreds creds)
        {
            Server = server;
            UserId = userId;
            Creds = creds;
        }
    }
}
