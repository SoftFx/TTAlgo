using TickTrader.Algo.Server.PublicAPI;

namespace TickTrader.Algo.ServerControl
{
    public class CredsChangedEvent
    {
        public ClientClaims.Types.AccessLevel AccessLevel { get; }


        public CredsChangedEvent(ClientClaims.Types.AccessLevel accessLevel)
        {
            AccessLevel = accessLevel;
        }
    }
}
