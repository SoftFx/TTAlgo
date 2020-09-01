using System.Linq;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common.Model.Setup
{
    public static class SymbolInfoExtensions
    {
        public static SymbolConfig ToConfig(this ISetupSymbolInfo symbol)
        {
            return new SymbolConfig { Name = symbol.Name, Origin = symbol.Origin };
        }

        public static SymbolKey ToInfo(this ISetupSymbolInfo symbol)
        {
            return new SymbolKey(symbol.Name, symbol.Origin);
        }

        public static SymbolKey ToKey(this SymbolInfo info)
        {
            return new SymbolKey(info.Name, SymbolOrigin.Online);
        }
    }


    public static class SymbolConfigExtensions
    {
        public static ISetupSymbolInfo ResolveInputSymbol(this SymbolConfig config, IAlgoSetupMetadata metadata, IAlgoSetupContext context, ISetupSymbolInfo mainSymbol)
        {
            ISetupSymbolInfo res = null;
            switch (config.Origin)
            {
                case SymbolOrigin.Token:
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

        public static ISetupSymbolInfo ResolveMainSymbol(this SymbolConfig config, IAlgoSetupMetadata metadata, IAlgoSetupContext context, ISetupSymbolInfo mainSymbol)
        {
            ISetupSymbolInfo res = null;
            if (config != null)
            {
                switch (config.Origin)
                {
                    case SymbolOrigin.Token:
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
