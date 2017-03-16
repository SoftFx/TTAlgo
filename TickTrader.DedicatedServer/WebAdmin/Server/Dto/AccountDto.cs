using TickTrader.Algo.Common.Model;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Dto
{
    public class AccountDto
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string Server { get; set; }
        
        public ConnectionErrorCodes LastConnectionStatus { get; set; }
    }
}
