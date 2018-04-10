namespace TickTrader.Algo.Common.Info
{
    public class SymbolInfo
    {
        public string Name { get; set; }


        public SymbolInfo() { }

        public SymbolInfo(string name)
        {
            Name = name;
        }


        public override string ToString()
        {
            return Name;
        }
    }
}
