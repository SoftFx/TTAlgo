using TickTrader.Algo.Api;

namespace TickTrader.Algo.Common.Info
{
    /// <summary>
    /// Default values for some plugin settings depending on placement target
    /// </summary>
    public class SetupContextInfo
    {
        public TimeFrames DefaultTimeFrame { get; set; }

        public SymbolInfo DefaultSymbol { get; set; }

        public MappingKey DefaultMapping { get; set; }


        public SetupContextInfo() { }

        public SetupContextInfo(TimeFrames defaultTimeFrame, SymbolInfo defaultSymbol, MappingKey defaultMapping)
        {
            DefaultTimeFrame = defaultTimeFrame;
            DefaultSymbol = defaultSymbol;
            DefaultMapping = defaultMapping;
        }
    }
}
