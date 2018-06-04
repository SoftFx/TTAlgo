using TickTrader.Algo.Common.Info;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class SetupMetadata
    {
        public ApiMetadataInfo Api { get; }

        public MappingCollectionInfo Mappings { get; }

        public AccountMetadataInfo Account { get; }

        public SetupContextInfo Context { get; }


        public SetupMetadata(ApiMetadataInfo api, MappingCollectionInfo mappings, AccountMetadataInfo account, SetupContextInfo context)
        {
            Api = api;
            Mappings = mappings;
            Account = account;
            Context = context;
        }

        public SetupMetadata(SetupMetadata metadata, AccountMetadataInfo account, SetupContextInfo context)
        {
            Api = metadata.Api;
            Mappings = metadata.Mappings;
            Account = account;
            Context = context;
        }

        public SetupMetadata(SetupMetadata metadata, SetupContextInfo context)
        {
            Api = metadata.Api;
            Mappings = metadata.Mappings;
            Account = metadata.Account;
            Context = context;
        }
    }
}
