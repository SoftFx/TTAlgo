using System.Collections.Generic;

namespace TickTrader.Algo.Server.Persistence
{
    internal class AccountSavedState
    {
        public string Id { get; set; }

        public string Server { get; set; }

        public string UserId { get; set; }

        public string DisplayName { get; set; }

        public Dictionary<string, string> Creds { get; private set; } = new Dictionary<string, string>();


        public AccountSavedState Clone()
        {
            return new AccountSavedState
            {
                Id = Id,
                Server = Server,
                UserId = UserId,
                DisplayName = DisplayName,
                Creds = new Dictionary<string, string>(Creds),
            };
        }
    }
}
