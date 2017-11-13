using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TickTrader.BotAgent.WebAdmin.Server.Core.Auth;

namespace TickTrader.BotAgent.WebAdmin.Server.Models
{
    public interface IAuthManager
    {
        ITokenOptions TokenOptions { get; set; }
        ClaimsIdentity Login(string login, string password);
        JwtSecurityToken GetJwt(ClaimsIdentity identity);
        string GetJwtString(ClaimsIdentity identity);
    }
}