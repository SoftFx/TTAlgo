namespace TickTrader.Algo.Common.Info
{
    public enum SymbolOrigin
    {
        Online = 0,
        Custom = 1,
        Special = 2,
    }


    public class SymbolInfo
    {
        public string Name { get; set; }

        public SymbolOrigin Origin { get; set; }


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
    }
}
