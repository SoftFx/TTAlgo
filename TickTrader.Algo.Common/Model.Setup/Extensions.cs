using System.Linq;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;

namespace TickTrader.Algo.Common.Model.Setup
{
    public static class ISymbolInfoExtensions
    {
        public static SymbolConfig ToConfig(this ISymbolInfo symbol)
        {
            return new SymbolConfig { Name = symbol.Name, Origin = symbol.Origin };
        }

        public static SymbolInfo ToInfo(this ISymbolInfo symbol)
        {
            return new SymbolInfo(symbol.Name, symbol.Origin);
        }
    }


    public static class SymbolConfigExtensions
    {
        public static ISymbolInfo ResolveInputSymbol(this SymbolConfig config, IAlgoSetupMetadata metadata, IAlgoSetupContext context, ISymbolInfo mainSymbol)
        {
            ISymbolInfo res = null;
            switch (config.Origin)
            {
                case SymbolOrigin.Special:
                    switch (config.Name)
                    {
                        case SpecialSymbols.MainSymbol:
                            res = mainSymbol;
                            break;
                    }
                    break;
                default:
                    res = metadata.Symbols.FirstOrDefault(s => s.Origin == config.Origin && s.Name == config.Name);
                    break;
            }
            return res ?? context.DefaultSymbol;
        }

        public static ISymbolInfo ResolveMainSymbol(this SymbolConfig config, IAlgoSetupMetadata metadata, IAlgoSetupContext context, ISymbolInfo mainSymbol)
        {
            ISymbolInfo res = null;
            if (config != null)
            {
                switch (config.Origin)
                {
                    case SymbolOrigin.Special:
                        switch (config.Name)
                        {
                            case SpecialSymbols.MainSymbol:
                                res = mainSymbol;
                                break;
                        }
                        break;
                    default:
                        res = metadata.Symbols.FirstOrDefault(s => s.Origin == config.Origin && s.Name == config.Name);
                        break;
                }
            }
            return res ?? context.DefaultSymbol;
        }
    }

}
