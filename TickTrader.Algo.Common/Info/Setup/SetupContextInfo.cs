using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Common.Info
{
    /// <summary>
    /// Default values for some plugin settings depending on placement target
    /// </summary>
    public class SetupContextInfo
    {
        public TimeFrames DefaultTimeFrame { get; set; }

        public SymbolKey DefaultSymbol { get; set; }

        public MappingKey DefaultMapping { get; set; }


        public SetupContextInfo() { }

        public SetupContextInfo(TimeFrames defaultTimeFrame, SymbolKey defaultSymbol, MappingKey defaultMapping)
        {
            DefaultTimeFrame = defaultTimeFrame;
            DefaultSymbol = defaultSymbol;
            DefaultMapping = defaultMapping;
        }
    }
}
