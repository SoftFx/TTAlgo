using TickTrader.Algo.Core;

namespace TickTrader.BotAgent.WebAdmin.Server.Dto
{
    public class TradeBotDto
    {
        public string Id { get; set; }
        public AccountDto Account { get; set; }
        public string State { get; set; }
        public string PackageName { get; internal set; }
        public string BotName { get; internal set; }
        public string FaultMessage { get; set; }
        public TradeBotConfigDto Config { get; set; }
        public PermissionsDto Permissions { get; set; }
        public bool Isolated { get; internal set; }
    }
}
