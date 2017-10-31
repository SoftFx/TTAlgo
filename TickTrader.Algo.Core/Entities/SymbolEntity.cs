using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using BO = TickTrader.BusinessObjects;
using BL = TickTrader.BusinessObjects;

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
        public string Description { get; set; }
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

        #region FDK Compatibility

        public double MinCommission { get; }
        public string MinCommissionCurrency { get; }
        //public BL.SwapType SwapType { get; }
        public int TripleSwapDay { get; }
        public double? DefaultSlippage { get; set; }
        public bool IsTradeEnabled { get; set; }
        public int GroupSortOrder { get; }
        public int CurrencySortOrder { get; }
        public int SettlementCurrencySortOrder { get; }
        public int CurrencyPrecision { get; }
        public int SettlementCurrencyPrecision { get; }
        public string StatusGroupId { get; }
        public string SecurityName => Security;
        public string SecurityDescription { get; }
        public double? HiddenLimitOrderMarginReduction { get; }
        public string Currency => BaseCurrencyCode;
        public string SettlementCurrency => CounterCurrencyCode;
        public int Precision => Digits;
        public double RoundLot => LotSize;
        public double MinTradeVolume => MinAmount;
        public double MaxTradeVolume => MaxAmount;
        public double TradeVolumeStep => AmountStep;
        public BL.ProfitCalculationModes ProfitCalcMode { get; }
        public BL.MarginCalculationModes MarginCalcMode { get; }
        public double MarginHedge { get; }
        public int MarginFactor { get; }
        public double ContractMultiplier { get; }
        public int Color { get; }

        #endregion
    }
}
