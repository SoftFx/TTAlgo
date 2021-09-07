namespace TickTrader.Algo.Domain
{
    public partial class SymbolConfig
    {
        public SymbolConfig(string name, Types.SymbolOrigin origin)
        {
            Name = name;
            Origin = origin;
        }
    }
}
