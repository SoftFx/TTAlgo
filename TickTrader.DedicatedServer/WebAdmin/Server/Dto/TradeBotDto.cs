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
        public string FaultMessage { get; set; }
    }
}
