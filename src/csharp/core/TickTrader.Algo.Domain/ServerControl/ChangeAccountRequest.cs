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

        public ChangeAccountRequest(string accountId, string password, string displayName = null) :
            this(accountId, string.IsNullOrEmpty(password) ? null : new AccountCreds(password), displayName)
        {

        }
    }
}
