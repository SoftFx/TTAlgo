namespace TickTrader.DedicatedServer.WebAdmin.Server.Dto
{
    public class TradeBotLogDto
    {
        public LogEntryDto[] Snapshot { get; set; }
        public string[] Files { get; set; }
    }
}
