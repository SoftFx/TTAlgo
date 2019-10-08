using ProtoBuf;
using System;
using TickTrader.Algo.Core;
using BO = TickTrader.BusinessObjects;

namespace TickTrader.Algo.Common.Model
{
    public enum CustomCommissionType { Percentage, Points, Money }

    [ProtoContract]
    public class CustomSymbol
    {
        [ProtoIgnore]
        internal Guid StorageId { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }
        [ProtoMember(3)]
        public string Description { get; set; }
        [ProtoMember(4)]
        public string BaseCurr { get; set; }
        [ProtoMember(5)]
        public string ProfitCurr { get; set; }
        [ProtoMember(6)]
        public int Digits { get; set; }
        [ProtoMember(7)]
        public double ContractSize { get; set; }
        [ProtoMember(8)]
        public double MinVolume { get; set; }
        [ProtoMember(9)]
        public double MaxVolume { get; set; }
        [ProtoMember(10)]
        public double VolumeStep { get; set; }
        [ProtoMember(11)]
        public double Commission { get; set; }

        [ProtoMember(12)]
        public bool SwapEnabled { get; set; }
        [ProtoMember(13)]
        public BO.SwapType SwapType { get; set; }
        [ProtoMember(14)]
        public double SwapSizeShort { get; set; }
        [ProtoMember(15)]
        public double SwapSizeLong { get; set; }
        [ProtoMember(16)]
        public bool TripleSwap { get; set; }

        [ProtoMember(17)]
        public BO.ProfitCalculationModes ProfitMode { get; set; }

        [ProtoMember(18)]
        public BO.MarginCalculationModes MarginMode { get; set; }
        [ProtoMember(19)]
        public double MarginHedged { get; set; }
        [ProtoMember(20)]
        public double MarginFactor { get; set; }
        [ProtoMember(21)]
        public double StopOrderMarginReduction { get; set; }
        [ProtoMember(22)]
        public double HiddenLimitOrderMarginReduction { get; set; }

        [ProtoMember(23)]
        public int Slippage { get; set; }

        [ProtoMember(24)]
        public CustomCommissionType CommissionType { get; set; }
        [ProtoMember(25)]
        public double LimitsCommission { get; set; }
        [ProtoMember(26)]
        public double MinCommission { get; set; }
        [ProtoMember(27)]
        public string MinCommissionCurr { get; set; }


        public SymbolEntity ToAlgo()
        {
            return new SymbolEntity(Name)
            {
                Description = Description,
                IsTradeAllowed = true,
                MinAmount = MinVolume,
                MaxAmount = MaxVolume,
                AmountStep = VolumeStep,
                LotSize = ContractSize,
                ContractSizeFractional = ContractSize,
                Digits = Digits,
                BaseCurrencyCode = BaseCurr,
                CounterCurrencyCode = ProfitCurr,
                DefaultSlippage = Slippage,

                Commission = Commission,
                CommissionType = Convert(CommissionType),
                CommissionChargeMethod = Api.CommissionChargeMethod.OneWay,
                CommissionChargeType = CommissionType != CustomCommissionType.Money ? Api.CommissionChargeType.PerTrade : Api.CommissionChargeType.PerLot,
                LimitsCommission = LimitsCommission,
                MinCommission = MinCommission,
                MinCommissionCurrency = MinCommissionCurr,

                SwapEnabled = SwapEnabled,
                SwapType = SwapType,
                SwapSizeLong = (float)SwapSizeLong,
                SwapSizeShort = (float)SwapSizeShort,
                TripleSwapDay = TripleSwap ? (int)DayOfWeek.Wednesday : 0,

                ProfitCalcMode = ProfitMode,

                MarginMode = MarginMode,
                MarginHedged = MarginHedged,
                MarginFactor = MarginFactor,
                StopOrderMarginReduction = StopOrderMarginReduction,
                HiddenLimitOrderMarginReduction = HiddenLimitOrderMarginReduction,
            };
        }

        public static CustomSymbol FromAlgo(SymbolEntity symbol)
        {
            return new CustomSymbol
            {
                Name = symbol.Name,
                Description = symbol.Description,
                BaseCurr = symbol.BaseCurrencyCode,
                ProfitCurr = symbol.CounterCurrencyCode,
                Digits = symbol.Digits,
                ContractSize = symbol.ContractSizeFractional,
                MinVolume = symbol.MinAmount,
                MaxVolume = symbol.MaxAmount,
                VolumeStep = symbol.AmountStep,
                Slippage = (int)(symbol.DefaultSlippage ?? 0),

                Commission = symbol.Commission,
                CommissionType = Convert(symbol.CommissionType),
                LimitsCommission = symbol.LimitsCommission,
                MinCommission = symbol.MinCommission,
                MinCommissionCurr = symbol.MinCommissionCurrency,

                SwapEnabled = symbol.SwapEnabled,
                SwapType = symbol.SwapType,
                SwapSizeLong = symbol.SwapSizeLong,
                SwapSizeShort = symbol.SwapSizeShort,
                TripleSwap = symbol.TripleSwapDay > 0,

                ProfitMode = symbol.ProfitCalcMode,

                MarginMode = symbol.MarginMode,
                MarginHedged = symbol.MarginHedged,
                MarginFactor = symbol.MarginFactor,
                StopOrderMarginReduction = symbol.StopOrderMarginReduction,
                HiddenLimitOrderMarginReduction = symbol.HiddenLimitOrderMarginReduction.Value
            };
        }

        private static CustomCommissionType Convert(Api.CommissionType type)
        {
            switch (type)
            {
                case Api.CommissionType.Absolute:
                    return CustomCommissionType.Money;

                case Api.CommissionType.Percent:
                    return CustomCommissionType.Percentage;

                case Api.CommissionType.PerUnit:
                    return CustomCommissionType.Points;

                default:
                    throw new InvalidCastException($"Commission type not found: {type}");
            }
        }

        private static Api.CommissionType Convert(CustomCommissionType type)
        {
            switch (type)
            {
                case CustomCommissionType.Money:
                    return Api.CommissionType.Absolute;

                case CustomCommissionType.Percentage:
                    return Api.CommissionType.Percent;

                case CustomCommissionType.Points:
                    return Api.CommissionType.PerUnit;

                default:
                    throw new InvalidCastException($"Commission type not found: {type}");
            }
        }
    }
}
