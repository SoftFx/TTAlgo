namespace TickTrader.BotAgent.WebAdmin.Server.Models
{
    public class BadRequestResult
    {
        public BadRequestResult(int code, string message)
        {
            Code = code;
            Message = message;
        }

        public int Code { get; set; }
        public string Message { get; set; }
    }
}
