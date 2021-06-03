namespace TickTrader.BotAgent.WebAdmin.Server.Models
{
    public class BadRequestResultDto
    {
        public BadRequestResultDto(int code, string message)
        {
            Code = code;
            Message = message;
        }

        public int Code { get; set; }
        public string Message { get; set; }
    }
}
