using TickTrader.Algo.Api;

namespace TickTrader.Algo.Common.Info
{
    /// <summary>
    /// Default values for some plugin settings depending on placement target
    /// </summary>
    public class SetupContextInfo
    {
        public TimeFrames DefaultTimeFrame { get; set; }

        public string DefaultSymbolCode { get; set; }

        public MappingKey DefaultMapping { get; set; }


        public SetupContextInfo() { }

        public SetupContextInfo(TimeFrames defaultTimeFrame, string defaultSymbolCode, MappingKey defaultMapping)
        {
            DefaultTimeFrame = defaultTimeFrame;
            DefaultSymbolCode = defaultSymbolCode;
            DefaultMapping = defaultMapping;
        }
    }
}
