namespace TickTrader.Algo.Domain.ServerControl
{
    public partial class ChangeAccountRequest
    {
        public ChangeAccountRequest(string accountId, AccountCreds creds, string displayName = null)
        {
            AccountId = accountId;
            Creds = creds;
            DisplayName = displayName;
        }
    }
}
