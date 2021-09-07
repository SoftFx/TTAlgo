namespace TickTrader.Algo.Domain.ServerControl
{
    public partial class RemoveAccountRequest
    {
        public RemoveAccountRequest(string accountId)
        {
            AccountId = accountId;
        }
    }
}
