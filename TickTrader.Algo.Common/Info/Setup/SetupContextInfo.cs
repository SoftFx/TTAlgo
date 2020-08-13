using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common.Info
{
    /// <summary>
    /// Default values for some plugin settings depending on placement target
    /// </summary>
    public class SetupContextInfo
    {
        public Feed.Types.Timeframe DefaultTimeFrame { get; set; }

        public SymbolKey DefaultSymbol { get; set; }

        public MappingKey DefaultMapping { get; set; }


        public SetupContextInfo() { }

        public SetupContextInfo(Feed.Types.Timeframe defaultTimeFrame, SymbolKey defaultSymbol, MappingKey defaultMapping)
        {
            DefaultTimeFrame = defaultTimeFrame;
            DefaultSymbol = defaultSymbol;
            DefaultMapping = defaultMapping;
        }
    }
}
