using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public enum AccountTypes
    {
        Gross,
        Net,
        Cash
    }

    public interface AccountDataProvider
    {
        double Balance { get; }
        double BalanceCurrency { get; }
        double Equity { get; }
        AccountTypes Type { get; }
        OrderList Orders { get; }
        NetPositionList NetPositions { get; }
        AssetList Assets { get; }
    }
}
