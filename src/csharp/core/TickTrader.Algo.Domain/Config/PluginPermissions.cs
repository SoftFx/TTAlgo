namespace TickTrader.Algo.Domain
{
    public partial class PluginPermissions
    {
        public string[] ToPermissionsList()
        {
            return new[]
            {
                $"{nameof(TradeAllowed)}: {TradeAllowed}",
                $"{nameof(Isolated)}: {Isolated}",
            };
        }
    }
}
