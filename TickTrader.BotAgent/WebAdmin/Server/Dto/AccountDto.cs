using TickTrader.Algo.Common.Info;

namespace TickTrader.BotAgent.WebAdmin.Server.Dto
{
    public class AccountDto
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string Server { get; set; }
        public bool UseNewProtocol { get; set; }

        public ConnectionErrorCodes LastConnectionStatus { get; set; }
    }
}
