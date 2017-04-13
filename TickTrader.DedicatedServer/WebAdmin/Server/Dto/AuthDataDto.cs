using System;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Dto
{
    public class AuthDataDto
    {
        public string Token { get; set; }
        public string User { get; set; }
        public DateTime Expires { get; set; }
    }
}
