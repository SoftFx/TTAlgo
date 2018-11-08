using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using TickTrader.Algo.Protocol;

namespace TickTrader.BotAgent.WebAdmin.Server.Protocol
{
    public class JwtProvider : IJwtProvider
    {
        public const string MinorVersionClaim = "minor";
        public const string AccessLevelClaim = "access";


        private SigningCredentials _signingCreds;
        private JwtSecurityTokenHandler _tokenHandler;
        private TokenValidationParameters _validationParams;


        public JwtProvider(string jwtKey)
        {
            var issuerKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey));
            _signingCreds = new SigningCredentials(issuerKey, SecurityAlgorithms.HmacSha256);
            _tokenHandler = new JwtSecurityTokenHandler();
            _validationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = issuerKey,
            };
        }


        public string CreateToken(Algo.Protocol.JwtPayload payload)
        {
            return _tokenHandler.WriteToken(GetToken(payload));
        }

        public Algo.Protocol.JwtPayload ParseToken(string token)
        {
            try
            {
                _tokenHandler.ValidateToken(token, _validationParams, out var securityToken);

                var jwtToken = (JwtSecurityToken)securityToken;
                var username = jwtToken.Claims.Last(c => c.Type == JwtRegisteredClaimNames.Sub);
                var sessionId = jwtToken.Claims.Last(c => c.Type == JwtRegisteredClaimNames.Jti);
                var minorVersion = jwtToken.Claims.Last(c => c.Type == MinorVersionClaim);
                var accessLevel = jwtToken.Claims.Last(c => c.Type == AccessLevelClaim);

                if (username == null)
                    throw new ArgumentException($"Missing claim '{nameof(username)}'");
                if (sessionId == null)
                    throw new ArgumentException($"Missing claim '{nameof(sessionId)}'");
                if (minorVersion == null)
                    throw new ArgumentException($"Missing claim '{nameof(minorVersion)}'");
                if (accessLevel == null)
                    throw new ArgumentException($"Missing claim '{nameof(accessLevel)}'");

                return new Algo.Protocol.JwtPayload
                {
                    Username = username.Value,
                    SessionId = sessionId.Value,
                    MinorVersion = int.Parse(minorVersion.Value),
                    AccessLevel = (AccessLevels)Enum.Parse(typeof(AccessLevels), accessLevel.Value),
                };

            }
            catch (SecurityTokenValidationException stvex)
            {
                throw new UnauthorizedException($"Token failed validation: {stvex.Message}");
            }
            catch (ArgumentException aex)
            {
                throw new UnauthorizedException($"Token was invalid: {aex.Message}");
            }
        }


        private JwtSecurityToken GetToken(Algo.Protocol.JwtPayload payload)
        {
            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, payload.Username),
                new Claim(JwtRegisteredClaimNames.Jti, payload.SessionId),
                new Claim(JwtRegisteredClaimNames.Iat, GetUnixEpochDate().ToString(), ClaimValueTypes.Integer64),
                new Claim(MinorVersionClaim, payload.MinorVersion.ToString(), ClaimValueTypes.Integer32),
                new Claim(AccessLevelClaim, payload.AccessLevel.ToString(), ClaimValueTypes.String),
            };

            return new JwtSecurityToken(
                issuer: "bot-agent.soft-fx.lv",
                audience: "BotTerminal",
                claims: claims,
                signingCredentials: _signingCreds);
        }


        private long GetUnixEpochDate()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUniversalTime().ToUnixTimeSeconds();
        }
    }
}
