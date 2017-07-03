namespace TickTrader.Algo.Core
{
    public class BotPermissions : IBotPermissions
    {
        public void AllowTrade(bool flag)
        {
            TradeAllowed = flag;
        }

        public bool TradeAllowed { get; private set; }
    }

    internal interface IBotPermissions: ITradePermissions
    { }

    internal interface ITradePermissions
    {
        bool TradeAllowed { get;  }
    }
}
