using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Server.Common;
using TickTrader.BotAgent.WebAdmin.Server.Extensions;

namespace TickTrader.BotAgent.WebAdmin.Server.Core.Auth
{
    public interface ISecurityTokenProvider
    {
        string CreateProtocolToken(Algo.ServerControl.JwtPayload payload);

        Algo.ServerControl.JwtPayload ValidateProtocolToken(string token);

        string CreateWebToken(ClaimsIdentity identity, out SecurityToken securityToken);

        ClaimsPrincipal ValidateWebToken(string token, out SecurityToken validatedToken);
    }


    public class JwtSecurityTokenProvider : JwtSecurityTokenHandler, ISecurityTokenProvider, ISecurityTokenValidator
    {
        private IConfiguration _config;
        private readonly SigningCredentials _singingCreds;


        public TokenValidationParameters WebValidationParams { get; }

        public TokenValidationParameters ProtocolValidationParams { get; }


        public JwtSecurityTokenProvider(IConfiguration config)
        {
            _config = config;
            var issuerKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(config.GetSecretKey()));
            _singingCreds = new SigningCredentials(issuerKey, SecurityAlgorithms.HmacSha256);

            WebValidationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                IssuerSigningKey = issuerKey,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            ProtocolValidationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = issuerKey,
            };
        }


        public string CreateProtocolToken(Algo.ServerControl.JwtPayload payload)
        {
            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, payload.Username),
                new Claim(JwtRegisteredClaimNames.Jti, payload.SessionId),
                new Claim(JwtRegisteredClaimNames.Iat, GetUnixEpochDate().ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtClaimNames.MinorVersionClaim, payload.MinorVersion.ToString(), ClaimValueTypes.Integer32),
                new Claim(JwtClaimNames.AccessLevelClaim, payload.AccessLevel.ToString(), ClaimValueTypes.String),
                new Claim(JwtClaimNames.CredsHashClaim, GetCredsHash(payload.Username)),
            };

            var securityToken = new JwtSecurityToken(
                issuer: "bot-agent.soft-fx.lv",
                audience: "BotTerminal",
                claims: claims,
                signingCredentials: _singingCreds);

            return WriteToken(securityToken);
        }

        public Algo.ServerControl.JwtPayload ValidateProtocolToken(string token)
        {
            try
            {
                ValidateToken(token, ProtocolValidationParams, out var securityToken);

                var jwtToken = (JwtSecurityToken)securityToken;
                var username = jwtToken.Claims.LastOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
                var sessionId = jwtToken.Claims.LastOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
                var minorVersion = jwtToken.Claims.LastOrDefault(c => c.Type == JwtClaimNames.MinorVersionClaim);
                var accessLevel = jwtToken.Claims.LastOrDefault(c => c.Type == JwtClaimNames.AccessLevelClaim);

                if (username == null)
                    throw new ArgumentException($"Missing claim '{nameof(username)}'");
                if (sessionId == null)
                    throw new ArgumentException($"Missing claim '{nameof(sessionId)}'");
                if (minorVersion == null)
                    throw new ArgumentException($"Missing claim '{nameof(minorVersion)}'");
                if (accessLevel == null)
                    throw new ArgumentException($"Missing claim '{nameof(accessLevel)}'");

                return new Algo.ServerControl.JwtPayload
                {
                    Username = username.Value,
                    SessionId = sessionId.Value,
                    MinorVersion = int.Parse(minorVersion.Value),
                    AccessLevel = (ClientClaims.Types.AccessLevel)Enum.Parse(typeof(ClientClaims.Types.AccessLevel), accessLevel.Value),
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

        public string CreateWebToken(ClaimsIdentity identity, out SecurityToken securityToken)
        {
            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, identity.Name),
                new Claim(JwtRegisteredClaimNames.Jti, GetUniqueId()),
                new Claim(JwtRegisteredClaimNames.Iat, GetUnixEpochDate().ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtClaimNames.CredsHashClaim, GetCredsHash(identity.Name)),
            };

            var now = DateTime.UtcNow;

            securityToken = new JwtSecurityToken(
                issuer: "bot-agent.soft-fx.lv",
                audience: "WebBrowser",
                claims: claims,
                notBefore: now,
                expires: now.Add(TimeSpan.FromMinutes(30)),
                signingCredentials: _singingCreds);

            return WriteToken(securityToken);
        }

        public ClaimsPrincipal ValidateWebToken(string token, out SecurityToken validatedToken)
        {
            return ValidateToken(token, WebValidationParams, out validatedToken);
        }

        public override ClaimsPrincipal ValidateToken(string token, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            var principal = base.ValidateToken(token, validationParameters, out validatedToken);

            var jwtToken = validatedToken as JwtSecurityToken;

            if (jwtToken == null)
                throw new SecurityTokenValidationException("SecurityToken is expected to be JwtSecurityToken");

            var username = jwtToken.Claims.LastOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            var credsHash = jwtToken.Claims.LastOrDefault(c => c.Type == JwtClaimNames.CredsHashClaim);

            if (username == null)
                throw new SecurityTokenValidationException($"Missing claim '{JwtRegisteredClaimNames.Sub}'");
            if (credsHash == null)
                throw new SecurityTokenValidationException($"Missing claim '{JwtClaimNames.CredsHashClaim}'");

            var validCreds = credsHash.Value == GetCredsHash(username.Value);

            if (!validCreds)
                throw new SecurityTokenValidationException("Credentials has changed");

            return principal;
        }


        private string GetCredsHash(string login)
        {
            var creds = _config.GetCredentials();
            string password = null;

            if (login == creds.AdminLogin)
                password = creds.AdminPassword;
            else if (login == creds.DealerLogin)
                password = creds.DealerPassword;
            else if (login == creds.ViewerLogin)
                password = creds.ViewerPassword;

            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes($"{login}{password}"));
                return string.Join("", bytes.Select(b => b.ToString("x2")));
            }
        }

        private long GetUnixEpochDate()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUniversalTime().ToUnixTimeSeconds();
        }

        private string GetUniqueId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
