using System;
using Microsoft.IdentityModel.Tokens;

namespace TickTrader.BotAgent.WebAdmin.Server.Core.Auth
{
    public class TokenOptions : ITokenOptions
    {
        public string Issuer { get; set; } = "bot-agent.soft-fx.lv";
        public string Audience { get; set; } = "WebBrowser";
        public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(30);
        public SigningCredentials SigningCredentials { get; set; }
        public Func<string> UniqIdGenerator { get; set; } = new Func<string>(() => Guid.NewGuid().ToString());
    }
}
