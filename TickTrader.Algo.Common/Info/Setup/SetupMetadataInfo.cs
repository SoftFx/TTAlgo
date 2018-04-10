namespace TickTrader.Algo.Common.Info
{
    public class SetupMetadataInfo
    {
        public ApiMetadataInfo Api { get; set; }

        public MappingCollectionInfo Mappings { get; set; }

        public AccountMetadataInfo Account { get; set; }


        public SetupMetadataInfo() { }

        public SetupMetadataInfo(ApiMetadataInfo api, MappingCollectionInfo mappings, AccountMetadataInfo account)
        {
            Account = account;
            Api = api;
            Mappings = mappings;
        }
    }
}
