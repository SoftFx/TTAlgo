﻿using System.Runtime.Serialization;

namespace TickTrader.Algo.Core.Config
{
    public enum SymbolOrigin
    {
        Online = 0,
        Custom = 1,
        Token = 2,
    }

    [DataContract(Name = "SymbolConfig", Namespace = "TTAlgo.Config.v2")]
    public class SymbolConfig
    {
        [DataMember(Name = "Name")]
        public string Name { get; set; }

        [DataMember(Name = "Origin")]
        public SymbolOrigin Origin { get; set; }


        public SymbolConfig Clone()
        {
            return new SymbolConfig
            {
                Name = Name,
                Origin = Origin,
            };
        }
    }


    [DataContract(Name = "Input", Namespace = "TTAlgo.Config.v2")]
    public abstract class Input : Property
    {
        [DataMember(Name = "Symbol")]
        public SymbolConfig SelectedSymbol { get; set; }


        public Input()
        {
            SelectedSymbol = new SymbolConfig();
        }
    }


    [DataContract(Name = "MappedInput", Namespace = "TTAlgo.Config.v2")]
    public abstract class MappedInput : Input
    {
        [DataMember(Name = "Mapping")]
        public MappingKey SelectedMapping { get; set; }
    }


    [DataContract(Name = "BarToBarInput", Namespace = "TTAlgo.Config.v2")]
    public class BarToBarInput : MappedInput
    {
        public override Property Clone()
        {
            return new BarToBarInput { Id = Id, SelectedSymbol = SelectedSymbol.Clone(), SelectedMapping = SelectedMapping.Clone() };
        }
    }


    [DataContract(Name = "BarToDoubleInput", Namespace = "TTAlgo.Config.v2")]
    public class BarToDoubleInput : MappedInput
    {
        public override Property Clone()
        {
            return new BarToDoubleInput { Id = Id, SelectedSymbol = SelectedSymbol.Clone(), SelectedMapping = SelectedMapping.Clone() };
        }
    }


    [DataContract(Name = "QuoteInput", Namespace = "TTAlgo.Config.v2")]
    public class QuoteInput : Input //TODO remove later
    {
        [DataMember(Name = "Level2")]
        public bool UseL2 { get; set; }


        public override Property Clone()
        {
            return new QuoteInput { Id = Id, SelectedSymbol = SelectedSymbol.Clone(), UseL2 = UseL2 };
        }
    }


    [DataContract(Name = "QuoteToBarInput", Namespace = "TTAlgo.Config.v2")]
    public class QuoteToBarInput : MappedInput //TODO remove later
    {
        public override Property Clone()
        {
            return new QuoteToBarInput { Id = Id, SelectedSymbol = SelectedSymbol.Clone(), SelectedMapping = SelectedMapping.Clone() };
        }
    }


    [DataContract(Name = "QuoteToDoubleInput", Namespace = "TTAlgo.Config.v2")]
    public class QuoteToDoubleInput : MappedInput //TODO remove later
    {
        public override Property Clone()
        {
            return new QuoteToDoubleInput { Id = Id, SelectedSymbol = SelectedSymbol.Clone(), SelectedMapping = SelectedMapping.Clone() };
        }
    }
}
