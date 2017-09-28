using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
using TickTrader.BusinessObjects;

namespace TickTrader.Algo.Common.Model
{
    [ProtoContract]
    public class CustomSymbol
    {
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


    public class CustomSymbolModel : ISymbolModel
    {
        private CustomSymbol _info;

        public CustomSymbolModel(Guid id, CustomSymbol info)
        {
            StorageId = id;
            _info = info;
        }

        public Guid StorageId { get; }
        public bool IsUserCreated => true;
        public string Description => _info.Description;
        public string Name => _info.Name;
        public string Security => "";

        public SymbolEntity GetAlgoSymbolInfo()
        {
            return _info.ToAlgo();
        }
    }
}
