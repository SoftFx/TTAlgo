using TickTrader.Algo.Common.Info;

namespace TickTrader.Algo.Common.Model.Setup
{
    public static class SpecialSymbols
    {
        public const string MainSymbol = "[main symbol]";


        public static SymbolToken MainSymbolPlaceholder => new SymbolToken(MainSymbol, SymbolOrigin.Special, null);
    }

    public class SymbolToken : ISymbolInfo
    {
        public string Name { get; set; }

        public SymbolOrigin Origin { get; set; }

        public string Id { get; set; }


        public SymbolToken(string name)
            : this(name, SymbolOrigin.Online, name)
        {

        }

        public SymbolToken(string name, SymbolOrigin origin)
            : this(name, origin, name)
        {

        }

        public SymbolToken(string name, SymbolOrigin origin, string id)
        {
            Name = name;
            Origin = origin;
            Id = id;
        }
    }
}
