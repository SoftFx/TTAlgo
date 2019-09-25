using ProtoBuf;
using System;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Common.Model
{
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
        public double Commission { get; set; } // taker fee in percentage for now

        public SymbolEntity ToAlgo()
        {
            return new SymbolEntity(Name)
            {
                Description = Description,
                IsTradeAllowed = true,
                SwapEnabled = false,
                MinAmount = MinVolume,
                MaxAmount = MaxVolume,
                AmountStep = VolumeStep,
                LotSize = ContractSize,
                ContractSizeFractional = ContractSize,
                Digits = Digits,
                BaseCurrencyCode = BaseCurr,
                CounterCurrencyCode = ProfitCurr,
                MarginHedged = 0.5,
                MarginFactorFractional = 1,
                Commission = Commission,
                CommissionType = Api.CommissionType.Percent,
                CommissionChargeMethod = Api.CommissionChargeMethod.OneWay,
                CommissionChargeType = Api.CommissionChargeType.PerTrade,
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
                Commission = symbol.Commission,
            };
        }

        //public CustomSymbol Clone()
        //{
        //    return new CustomSymbol()
        //    {
        //        Id = Id,
        //        Name = Name,
        //        Description = Description,
        //    };
        //}
    }
}
