using TickTrader.Algo.Common.Info;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class SymbolToken : ISymbolInfo
    {
        public static SymbolToken CreateMainSymbol()
        {
            return new SymbolToken("[main symbol]", SymbolOrigin.Special);
        }

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
