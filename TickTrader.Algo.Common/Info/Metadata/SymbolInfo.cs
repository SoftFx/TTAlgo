﻿using TickTrader.Algo.Common.Model.Setup;

namespace TickTrader.Algo.Common.Info
{
    public enum SymbolOrigin
    {
        Online = 0,
        Custom = 1,
        Special = 2,
    }


    public class SymbolInfo : ISymbolInfo
    {
        public string Name { get; set; }

        public SymbolOrigin Origin { get; set; }

        public string Id => Name;

        public SymbolInfo() { }

        public SymbolInfo(string name, SymbolOrigin origin)
        {
            Name = name;
            Origin = origin;
        }


        public override string ToString()
        {
            return $"{Name} ({Origin})";
        }

        public override int GetHashCode()
        {
            return $"{Name}{Origin}".GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var symbol = obj as SymbolInfo;
            return symbol != null
                && symbol.Origin == Origin
                && symbol.Name == Name;
        }
    }
}
