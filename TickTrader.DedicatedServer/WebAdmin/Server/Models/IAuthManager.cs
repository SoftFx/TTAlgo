using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TickTrader.DedicatedServer.WebAdmin.Server.Core.Auth;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Models
{
    public interface IAuthManager
    {
        ITokenOptions TokenOptions { get; set; }
        ClaimsIdentity Login(string login, string password);
        JwtSecurityToken GetJwt(ClaimsIdentity identity);
        string GetJwtString(ClaimsIdentity identity);
    }
}