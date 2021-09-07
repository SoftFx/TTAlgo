using TickTrader.Algo.Domain;

namespace TickTrader.BotAgent.WebAdmin.Server.Dto
{
    public class AccountDto
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string Server { get; set; }

        public ConnectionErrorInfo.Types.ErrorCode LastConnectionStatus { get; set; }
    }
}
