using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class AlgoSetupContextStub : IAlgoSetupContext
    {
        private SetupContextInfo _info;
        private SymbolToken _token;


        public Feed.Types.Timeframe DefaultTimeFrame => _info.DefaultTimeFrame;

        public ISetupSymbolInfo DefaultSymbol => _token;

        public MappingKey DefaultMapping => _info.DefaultMapping;


        public AlgoSetupContextStub(SetupContextInfo info)
        {
            _info = info;
            _token = new SymbolToken(_info.DefaultSymbol.Name, _info.DefaultSymbol.Origin);
        }
    }
}
