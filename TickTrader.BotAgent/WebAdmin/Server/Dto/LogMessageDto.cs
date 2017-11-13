using System;
using TickTrader.BotAgent.BA;

namespace TickTrader.BotAgent.WebAdmin.Server.Dto
{
    public class LogEntryDto
    {
        public DateTime Time { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
    }
}
