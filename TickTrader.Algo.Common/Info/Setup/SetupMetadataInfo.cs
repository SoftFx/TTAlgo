namespace TickTrader.Algo.Common.Info
{
    /// <summary>
    /// General setup metadata, same for all plugins on BotTeeminal, BotAgent
    /// </summary>
    public class SetupMetadataInfo
    {
        public ApiMetadataInfo Api { get; set; }

        public MappingCollectionInfo Mappings { get; set; }


        public SetupMetadataInfo() { }

        public SetupMetadataInfo(ApiMetadataInfo api, MappingCollectionInfo mappings)
        {
            Api = api;
            Mappings = mappings;
        }
    }
}
