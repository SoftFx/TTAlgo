namespace TickTrader.Algo.Domain
{
    public partial class SetupContextInfo
    {
        public SetupContextInfo(Feed.Types.Timeframe defaultTimeframe, SymbolConfig defaultSymbol, MappingKey defaultMapping)
        {
            DefaultTimeFrame = defaultTimeframe;
            DefaultSymbol = defaultSymbol;
            DefaultMapping = defaultMapping;
        }
    }
}
