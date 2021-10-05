﻿using TickTrader.Algo.Domain.ServerControl;

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