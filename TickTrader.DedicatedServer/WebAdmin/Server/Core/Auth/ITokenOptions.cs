using System;
using Microsoft.IdentityModel.Tokens;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Core.Auth
{
    public interface ITokenOptions
    {
        string Issuer { get; set; }
        string Audience { get; set; }
        TimeSpan Expiration { get; set; }
        SigningCredentials SigningCredentials { get; set; }
        Func<string> UniqIdGenerator { get; set; }
    }
}
