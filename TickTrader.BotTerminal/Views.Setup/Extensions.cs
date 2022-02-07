using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Core.Setup;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;

namespace TickTrader.BotTerminal
{
    public static class AccountMetadataInfoExtensions
    {
        public static IReadOnlyList<StorageSymbolKey> GetAvaliableSymbols(this AccountMetadataInfo metadata, StorageSymbolKey defaultSymbol)
        {
            var symbols = metadata?.Symbols.OrderBy(u => u.Name).Select(c => new StorageSymbolKey(c.Name, c.Origin)).ToList();
            if ((symbols?.Count ?? 0) == 0)
                symbols = new List<StorageSymbolKey> { defaultSymbol };

            if (!symbols.ContainMainToken())
                symbols.Insert(0, SpecialSymbols.MainSymbolPlaceholder.GetKey());

            return symbols;
        }

        public static StorageSymbolKey GetSymbolOrDefault(this IReadOnlyList<StorageSymbolKey> availableSymbols, StorageSymbolKey config)
        {
            if (config != null)
                return availableSymbols.FirstOrDefault(s => s.Origin == config.Origin && s.Name == config.Name);
            return null;
        }

        public static StorageSymbolKey GetSymbolOrAny(this IReadOnlyList<StorageSymbolKey> availableSymbols, StorageSymbolKey info)
        {
            return availableSymbols.FirstOrDefault(s => s.Origin == info.Origin && s.Name == info.Name)
                ?? availableSymbols.First();
        }

        public static bool ContainMainToken(this IReadOnlyList<StorageSymbolKey> availableSymbols)
        {
            return availableSymbols.FirstOrDefault(u => u.Name == SpecialSymbols.MainSymbol && u.Origin == SymbolConfig.Types.SymbolOrigin.Token) != null;
        }
    }


    public static class SymbolInfoExtensions
    {
        public static SymbolConfig ToConfig(this SymbolKey info)
        {
            return new SymbolConfig { Name = info.Name, Origin = info.Origin };
        }
    }


    public static class SetupContextExtensions
    {
        public static SetupContextInfo GetSetupContextInfo(this IAlgoSetupContext setupContext)
        {
            return new SetupContextInfo(setupContext.DefaultTimeFrame, setupContext.DefaultSymbol.ToConfig(), setupContext.DefaultMapping);
        }
    }

    public class StorageSymbolKey : SymbolKey, ISymbolKey
    {
        public StorageSymbolKey(string name, SymbolConfig.Types.SymbolOrigin origin) : base(name, origin) { }

        bool IEqualityComparer<ISymbolKey>.Equals(ISymbolKey x, ISymbolKey y) => x.Name == y.Name && x.Origin == y.Origin && x.Id == y.Id;

        int IEqualityComparer<ISymbolKey>.GetHashCode(ISymbolKey obj) => base.GetHashCode();
    }


    public static class StorageSymbolKeyExtensions
    {
        public static StorageSymbolKey ToKey(this SymbolConfig info)
        {
            return new StorageSymbolKey(info.Name, SymbolConfig.Types.SymbolOrigin.Online);
        }

        public static StorageSymbolKey ToKey(this ISymbolKey info)
        {
            return new StorageSymbolKey(info.Name, info.Origin);
        }
    }

    public static class SpecialSymbols
    {
        public const string MainSymbol = "[Main Symbol]";


        public static SymbolToken MainSymbolPlaceholder => new SymbolToken(MainSymbol, SymbolConfig.Types.SymbolOrigin.Token, null);

        public static StorageSymbolKey GetKey(this ISetupSymbolInfo info)
        {
            return new StorageSymbolKey(info.Name, info.Origin);
        }
    }
}
