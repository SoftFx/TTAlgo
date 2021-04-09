namespace TickTrader.Algo.Domain.ServerControl
{
    public partial class AddPluginRequest
    {
        public AddPluginRequest(string accountId, PluginConfig config)
        {
            AccountId = accountId;
            Config = config;
        }
    }
}
