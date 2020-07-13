namespace TickTrader.Algo.Domain
{
    public partial class AssetInfo
    {
        public AssetInfo(double balance, string currency)
        {
            Balance = balance;
            Currency = currency;
        }
    }
}
