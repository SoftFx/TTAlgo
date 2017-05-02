using System.Collections.Generic;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Dto
{
    public class TradeBotDto
    {
        public string Id { get; set; }
        public bool IsRunning { get; set; }
        public string Status { get; set; }
        public AccountDto Account { get; set; }
        public string State { get; set; }
        public TradeBotConfigDto Config { get; set; }
    }
}
