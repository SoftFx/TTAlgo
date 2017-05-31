using System.Collections.Generic;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Dto
{
    public class TradeBotConfigDto
    {
        public string Symbol { get; set; }
        public IEnumerable<ParameterDto> Parameters { get; set; }
    }
}
