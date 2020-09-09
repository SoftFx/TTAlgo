namespace TickTrader.Algo.Domain
{
    public partial class PluginPermissions
    {
        partial void OnConstruction()
        {
            Reset();
        }

        public void Reset()
        {
            TradeAllowed = true;
            Isolated = true;
        }
    }
}
