using System;
using System.Collections.Generic;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Common.Model.Setup;

namespace TickTrader.Algo.Common.Info
{
    public enum SymbolOrigin
    {
        Online = 0,
        Custom = 1,
        Token = 2,
    }

    public class SymbolKey : ISetupSymbolInfo, IComparable
    {
        public string Name { get; set; }

        public SymbolOrigin Origin { get; set; }

        public string Id => Name;

        public SymbolKey(string name, SymbolOrigin origin)
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
            return HashCode.GetComposite(Name, Origin);
        }

        public override bool Equals(object obj)
        {
            var symbol = obj as SymbolKey;
            return symbol != null
                && symbol.Origin == Origin
                && symbol.Name == Name;
        }

        public int CompareTo(object obj)
        {
            var other = obj as SymbolKey;
            if (other == null)
                return -1;

            if (Origin == other.Origin)
                return Name.CompareTo(other.Name);
            else
                return Origin.CompareTo(other.Origin);
        }
    }

    public class SymbolKeyComparer : IComparer<SymbolKey>
    {
        public int Compare(SymbolKey x, SymbolKey y)
        {
            if (x.Origin == y.Origin)
                return x.Name.CompareTo(y.Name);
            else
                return y.Origin.CompareTo(x.Origin);
        }
    }
}
