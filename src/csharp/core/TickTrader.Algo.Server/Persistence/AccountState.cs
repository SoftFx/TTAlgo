using System.Collections.Generic;

namespace TickTrader.Algo.Server.Persistence
{
    internal class AccountState
    {
        public string Id { get; set; }

        public string Server { get; set; }

        public string UserId { get; set; }

        public string DisplayName { get; set; }

        public Dictionary<string, string> Creds { get; set; }
    }
}
