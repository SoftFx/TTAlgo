using System;
using TickTrader.DedicatedServer.DS;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Dto
{
    public class LogEntryDto
    {
        public DateTime Time { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
    }
}
