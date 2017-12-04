using System.Collections.Generic;

namespace TickTrader.BotAgent.WebAdmin.Server.Dto
{
    public class TradeBotConfigDto
    {
        public string Symbol { get; set; }
        public IEnumerable<ParameterDto> Parameters { get; set; }
    }
}
