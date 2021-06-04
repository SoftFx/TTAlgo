using ProtoBuf;
using System;

namespace TickTrader.FeedStorage
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
        public Algo.Domain.SwapInfo.Types.Type SwapType { get; set; }
        [ProtoMember(14)]
        public double SwapSizeShort { get; set; }
        [ProtoMember(15)]
        public double SwapSizeLong { get; set; }
        [ProtoMember(16)]
        public bool TripleSwap { get; set; }

        [ProtoMember(17)]
        public Algo.Domain.MarginInfo.Types.CalculationMode ProfitMode { get; set; }

        [ProtoMember(18)]
        public Algo.Domain.MarginInfo.Types.CalculationMode MarginMode { get; set; }
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


        public Algo.Domain.SymbolInfo ToAlgo()
        {
            return new Algo.Domain.SymbolInfo
            {
                Name = Name,
                TradeAllowed = true,
                BaseCurrency = BaseCurr,
                CounterCurrency = ProfitCurr,
                Digits = Digits,
                LotSize = ContractSize,
                MinTradeVolume = MinVolume,
                MaxTradeVolume = MaxVolume,
                TradeVolumeStep = VolumeStep,

                Description = Description,

                Slippage = new Algo.Domain.SlippageInfo
                {
                    DefaultValue = Slippage,
                    Type = Algo.Domain.SlippageInfo.Types.Type.Pips,
                },

                Commission = new Algo.Domain.CommissonInfo
                {
                    Commission = Commission,
                    LimitsCommission = LimitsCommission,
                    ValueType = Convert(CommissionType),
                    MinCommission = MinCommission,
                    MinCommissionCurrency = MinCommissionCurr,
                },

                Swap = new Algo.Domain.SwapInfo
                {
                    Enabled = SwapEnabled,
                    Type = SwapType,
                    SizeLong = SwapSizeLong,
                    SizeShort = SwapSizeShort,
                    TripleSwapDay = TripleSwap ? (int)DayOfWeek.Wednesday : 0,
                },

                Margin = new Algo.Domain.MarginInfo
                {
                    Mode = MarginMode,
                    Factor = MarginFactor,
                    Hedged = MarginHedged,
                    StopOrderReduction = StopOrderMarginReduction,
                    HiddenLimitOrderReduction = HiddenLimitOrderMarginReduction,
                },
            };
        }

        public static CustomSymbol FromAlgo(Algo.Domain.SymbolInfo symbol)
        {
            return new CustomSymbol
            {
                Name = symbol.Name,
                BaseCurr = symbol.BaseCurrency,
                ProfitCurr = symbol.CounterCurrency,
                Digits = symbol.Digits,
                ContractSize = symbol.LotSize,
                MinVolume = symbol.MinTradeVolume,
                MaxVolume = symbol.MaxTradeVolume,
                VolumeStep = symbol.TradeVolumeStep,
                Slippage = (int)(symbol.Slippage.DefaultValue ?? 0),
                Description = symbol.Description,

                Commission = symbol.Commission.Commission,
                LimitsCommission = symbol.Commission.LimitsCommission,
                CommissionType = Convert(symbol.Commission.ValueType),
                MinCommission = symbol.Commission.MinCommission,
                MinCommissionCurr = symbol.Commission.MinCommissionCurrency,

                SwapEnabled = symbol.Swap.Enabled,
                SwapType = symbol.Swap.Type,
                SwapSizeLong = symbol.Swap.SizeLong ?? 0,
                SwapSizeShort = symbol.Swap.SizeShort ?? 0,
                TripleSwap = symbol.Swap.TripleSwapDay > 0,

                MarginMode = symbol.Margin.Mode,
                MarginHedged = symbol.Margin.Hedged,
                MarginFactor = symbol.Margin.Factor,
                StopOrderMarginReduction = symbol.Margin.StopOrderReduction ?? 1,
                HiddenLimitOrderMarginReduction = symbol.Margin.HiddenLimitOrderReduction ?? 1
            };
        }

        private static CustomCommissionType Convert(Algo.Domain.CommissonInfo.Types.ValueType type)
        {
            switch (type)
            {
                case Algo.Domain.CommissonInfo.Types.ValueType.Money:
                    return CustomCommissionType.Money;

                case Algo.Domain.CommissonInfo.Types.ValueType.Percentage:
                    return CustomCommissionType.Percentage;

                case Algo.Domain.CommissonInfo.Types.ValueType.Points:
                    return CustomCommissionType.Points;

                default:
                    throw new InvalidCastException($"Commission type not found: {type}");
            }
        }

        private static Algo.Domain.CommissonInfo.Types.ValueType Convert(CustomCommissionType type)
        {
            switch (type)
            {
                case CustomCommissionType.Money:
                    return Algo.Domain.CommissonInfo.Types.ValueType.Money;

                case CustomCommissionType.Percentage:
                    return Algo.Domain.CommissonInfo.Types.ValueType.Percentage;

                case CustomCommissionType.Points:
                    return Algo.Domain.CommissonInfo.Types.ValueType.Points;

                default:
                    throw new InvalidCastException($"Commission type not found: {type}");
            }
        }
    }
}
