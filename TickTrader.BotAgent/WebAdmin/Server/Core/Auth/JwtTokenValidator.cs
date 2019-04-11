using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TickTrader.BotAgent.WebAdmin.Server.Extensions;

namespace TickTrader.BotAgent.WebAdmin.Server.Core.Auth
{
    public class JwtTokenValidator : JwtSecurityTokenHandler, ISecurityTokenValidator
    {
        private readonly IConfiguration _config;


        public JwtTokenValidator(IConfiguration config)
        {
            _config = config;
        }


        public override ClaimsPrincipal ValidateToken(string token, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            validationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config.GetJwtKey()));
            return base.ValidateToken(token, validationParameters, out validatedToken);
        }
    }
}
