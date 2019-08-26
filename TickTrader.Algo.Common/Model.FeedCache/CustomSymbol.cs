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

        public SymbolEntity ToAlgo()
        {
            return new SymbolEntity(Name);
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
