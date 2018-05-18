using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Setup;

namespace TickTrader.BotTerminal
{
    internal class AlgoSetupContextStub : IAlgoSetupContext
    {
        private SetupContextInfo _info;


        public TimeFrames DefaultTimeFrame => _info.DefaultTimeFrame;

        public string DefaultSymbolCode => _info.DefaultSymbolCode;

        public MappingKey DefaultMapping => _info.DefaultMapping;


        public AlgoSetupContextStub(SetupContextInfo info)
        {
            _info = info;
        }
    }
}
