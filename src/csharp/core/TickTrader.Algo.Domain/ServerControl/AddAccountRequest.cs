namespace TickTrader.Algo.Domain.ServerControl
{
    public partial class AddAccountRequest
    {
        public AddAccountRequest(string server, string userId, AccountCreds creds, string displayName = null)
        {
            Server = server;
            UserId = userId;
            Creds = creds;
            DisplayName = displayName;
        }
    }
}
