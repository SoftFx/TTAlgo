using System;
using Microsoft.IdentityModel.Tokens;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Core.Auth
{
    public class TokenOptions : ITokenOptions
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(30);
        public SigningCredentials SigningCredentials { get; set; }
        public Func<string> UniqIdGenerator { get; set; } = new Func<string>(() => Guid.NewGuid().ToString());
    }
}
