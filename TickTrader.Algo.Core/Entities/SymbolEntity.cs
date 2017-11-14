using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using BO = TickTrader.BusinessObjects;

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
        public double ContractSizeFractional { get; set; }
        public double MarginFactorFractional { get; set; }
        public double StopOrderMarginReduction { get; set; }
        public double MarginHedged { get; set; }
        public BO.MarginCalculationModes MarginMode { get; set; }
        public bool SwapEnabled { get; set; }
        public float SwapSizeLong { get; set; }
        public float SwapSizeShort { get; set; }
        public string Security { get; set; }
        public int SortOrder { get; set; }
        public int GroupSortOrder { get; set; }
        public BO.SwapType SwapType { get; set; }
        public int TripleSwapDay { get; set; }
        public double HiddenLimitOrderMarginReduction { get; set; }

    }
}
