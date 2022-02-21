using ProtoBuf;
using System;
using TickTrader.Algo.Domain;

namespace TickTrader.FeedStorage
{
    [ProtoContract]
    public class CustomCurrency
    {
        [ProtoIgnore]
        internal Guid StorageId { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }
        [ProtoMember(3)]
        public int Digits { get; set; }


        public static CustomCurrency FromAlgo(CurrencyInfo currency)
        {
            return new CustomCurrency { Name = currency.Name, Digits = currency.Digits };
        }


        public CurrencyInfo ToAlgo()
        {
            return new CurrencyInfo { Name = Name, Digits = Digits };
        }
    }
}
