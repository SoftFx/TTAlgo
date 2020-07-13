using System.Collections.Generic;

namespace TickTrader.Algo.Domain
{
    public partial class AccountInfo
    {
        public bool IsMarginal => Type != Types.Type.Cash;

        public double Balance => IsMarginal && Assets.Count > 0 ? Assets[0].Balance : 0.0;

        public string BalanceCurrency => IsMarginal && Assets.Count > 0 ? Assets[0].Currency : string.Empty;


        /// <summary>
        /// Initializes assets collection according to balance value
        /// </summary>
        /// <param name="balance">Should be null for cash accounts</param>
        /// <param name="balanceCurrency"></param>
        /// <param name="assets">Can be null for marginal accounts</param>
        public AccountInfo(double? balance, string balanceCurrency, IEnumerable<AssetInfo> assets)
        {
            if (balance.HasValue)
            {
                // marginal accounts
                Assets.Add(new AssetInfo { Balance = balance.Value, Currency = balanceCurrency });
            }
            else
            {
                // cash accounts
                Assets.AddRange(assets);
            }
        }
    }
}
