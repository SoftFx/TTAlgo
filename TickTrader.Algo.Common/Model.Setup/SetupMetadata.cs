using System.Linq;
using TickTrader.Algo.Common.Info;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class SetupMetadata
    {
        public ApiMetadataInfo Api { get; }

        public MappingCollectionInfo Mappings { get; }

        public AccountMetadataInfo Account { get; }

        public SetupContextInfo Context { get; }

        public SymbolInfo DefaultSymbol { get; }


        public SetupMetadata(ApiMetadataInfo api, MappingCollectionInfo mappings, AccountMetadataInfo account, SetupContextInfo context)
        {
            Api = api;
            Mappings = mappings;
            Account = account;
            Context = context;

            DefaultSymbol = account.Symbols == null ? context.DefaultSymbol
                : (account.Symbols.Any(s => s.Equals(context.DefaultSymbol)) ? context.DefaultSymbol : account.DefaultSymbol);
        }

        public SetupMetadata(SetupMetadata metadata, AccountMetadataInfo account, SetupContextInfo context)
            : this(metadata.Api, metadata.Mappings, account, context)
        {
        }

        public SetupMetadata(SetupMetadata metadata, SetupContextInfo context)
            : this(metadata.Api, metadata.Mappings, metadata.Account, context)
        {
        }
    }
}
