namespace TickTrader.Algo.Domain
{
    public static class SharedConstants
    {
        public const char IdSeparator = '/';
        public static readonly string IdSeparatorStr = new string(IdSeparator, 1);
        public const string InvalidIdPart = "<invalid>";

        public const string PackageIdPrefix = "pkg";
        public const string EmbeddedRepositoryId = "";
        public const string LocalRepositoryId = "local";
        public const string CommonRepositoryId = "common";

        public const string AccountIdPrefix = "acc";

        public const string RuntimeIdPrefix = "rt";
    }
}
