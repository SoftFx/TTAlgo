using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class SymbolEntity
    {
        public SymbolEntity(string code)
        {
            this.Name = code;
        }

        public string Name { get; private set; }
        public int Digits { get; set; }
        public double LotSize { get; set; }
        public double MaxAmount { get; set; }
        public double MinAmount { get; set; }
        public double AmountStep { get; set; }
        public string BaseCurrencyCode { get; set; }
        public string CounterCurrencyCode { get; set; }
        public bool IsTradeAllowed { get; set; }
        public double Commission { get; set; }
        public double LimitsCommission { get; set; }
        public CommissionChargeMethod CommissionChargeMethod { get; set; }
        public CommissionChargeType CommissionChargeType { get; set; }
        public CommissionType CommissionType { get; set; }
    }
}
