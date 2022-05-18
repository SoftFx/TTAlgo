namespace TickTrader.Algo.Server.PublicAPI
{
    public sealed class ApiAccessManager : AccessManager, IAccessManager
    {
        public new ClientClaims.Types.AccessLevel Level { get; }


        public ApiAccessManager(ClientClaims.Types.AccessLevel level) : base()
        {
            Level = level;

            HasViewerAccess = Level == ClientClaims.Types.AccessLevel.Viewer ||
                              Level == ClientClaims.Types.AccessLevel.Dealer ||
                              Level == ClientClaims.Types.AccessLevel.Admin;

            HasDealerAccess = Level == ClientClaims.Types.AccessLevel.Dealer ||
                              Level == ClientClaims.Types.AccessLevel.Admin;

            HasAdminAccess = Level == ClientClaims.Types.AccessLevel.Admin;
        }
    }
}
