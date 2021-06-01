using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Setup
{
    public static class SpecialSymbols
    {
        public const string MainSymbol = "[Main Symbol]";


        public static SymbolToken MainSymbolPlaceholder => new SymbolToken(MainSymbol, SymbolConfig.Types.SymbolOrigin.Token, null);

        public static SymbolKey GetKey(this ISetupSymbolInfo info)
        {
            return new SymbolKey(info.Name, info.Origin);
        }
    }

    public class SymbolToken : ISetupSymbolInfo
    {
        public string Name { get; set; }

        public SymbolConfig.Types.SymbolOrigin Origin { get; set; }

        public string Id { get; set; }

        public SymbolToken(string name)
            : this(name, SymbolConfig.Types.SymbolOrigin.Online, name)
        {

        }

        public SymbolToken(string name, SymbolConfig.Types.SymbolOrigin origin)
            : this(name, origin, name)
        {

        }

        public SymbolToken(string name, SymbolConfig.Types.SymbolOrigin origin, string id)
        {
            Name = name;
            Origin = origin;
            Id = id;
        }

        public override bool Equals(object obj)
        {
            var otherInfo = obj as ISetupSymbolInfo;
            return otherInfo != null && otherInfo.Origin == Origin
                && otherInfo.Name == Name && otherInfo.Id == Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name.GetHashCode(), Origin.GetHashCode());
        }
    }
}
