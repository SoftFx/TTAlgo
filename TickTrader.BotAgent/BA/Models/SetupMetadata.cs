using TickTrader.Algo.Core.Setup;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Package;

namespace TickTrader.BotAgent.BA.Models
{
    public class StubSymbolInfo : ISetupSymbolInfo
    {
        public string Name { get; set; }

        public SymbolConfig.Types.SymbolOrigin Origin { get; set; }

        public string Id => Name;


        public StubSymbolInfo(SymbolKey symbolInfo)
        {
            Name = symbolInfo.Name;
            Origin = SymbolConfig.Types.SymbolOrigin.Online;
        }
    }


    // stub
    public class SetupContext : IAlgoSetupContext
    {
        private static MappingKey _defaultMapping = MappingDefaults.DefaultBarToBarMapping.Key;
        private static SymbolToken _defaultSymbol = new SymbolToken("none");


        public Feed.Types.Timeframe DefaultTimeFrame => Feed.Types.Timeframe.M1;

        public ISetupSymbolInfo DefaultSymbol => _defaultSymbol;

        public MappingKey DefaultMapping => _defaultMapping;
    }
}
