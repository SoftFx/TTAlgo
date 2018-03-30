using TickTrader.Algo.Api;

namespace TickTrader.Algo.Common.Info
{
    public class SetupContextInfo
    {
        public TimeFrames DefaultTimeFrame { get; set; }

        public string DefaultSymbolCode { get; set; }

        public string DefaultMapping { get; set; }


        public SetupContextInfo(TimeFrames defaultTimeFrame, string defaultSymbolCode, string defaultMapping)
        {
            DefaultTimeFrame = defaultTimeFrame;
            DefaultSymbolCode = defaultSymbolCode;
            DefaultMapping = defaultMapping;
        }
    }
}
