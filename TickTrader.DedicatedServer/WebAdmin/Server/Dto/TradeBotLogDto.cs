﻿namespace TickTrader.DedicatedServer.WebAdmin.Server.Dto
{
    public class TradeBotLogDto
    {
        public LogEntryDto[] Snapshot { get; set; }
        public FileDto[] Files { get; set; }
    }
}
