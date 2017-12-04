using System;
using Microsoft.IdentityModel.Tokens;

namespace TickTrader.BotAgent.WebAdmin.Server.Core.Auth
{
    public interface ITokenOptions
    {
        string Issuer { get; }
        string Audience { get; }
        TimeSpan Expiration { get; }
        SigningCredentials SigningCredentials { get; }
        Func<string> UniqIdGenerator { get; }
    }
}
