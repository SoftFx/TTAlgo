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
        string Id { get; }
        double Balance { get; }
        string BalanceCurrency { get; }
        double Equity { get; }
        AccountTypes Type { get; }
        OrderList Orders { get; }
        NetPositionList NetPositions { get; }
        AssetList Assets { get; }

        event Action BalanceUpdated;
    }
}
