namespace TickTrader.Algo.Common.Info
{
    public class CurrencyInfo
    {
        public string Name { get; set; }


        public CurrencyInfo() { }

        public CurrencyInfo(string name)
        {
            Name = name;
        }


        public override string ToString()
        {
            return Name;
        }
    }
}
