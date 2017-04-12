﻿using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using TickTrader.DedicatedServer.WebAdmin.Server.Core.Auth;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Models
{
    public class AuthManager : IAuthManager
    {
        public AuthManager(ITokenOptions tokenOptions)
        {
            TokenOptions = tokenOptions;
        }

        public ITokenOptions TokenOptions { get; set; }

        public JwtSecurityToken GetJwt(ClaimsIdentity identity)
        {
            var now = DateTime.UtcNow;

            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, identity.Name),
                new Claim(JwtRegisteredClaimNames.Jti, TokenOptions.UniqIdGenerator()),
                new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(now).ToString(), ClaimValueTypes.Integer64)
            }.Union(identity.Claims);

            return new JwtSecurityToken(
                issuer: TokenOptions.Issuer,
                audience: TokenOptions.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(TokenOptions.Expiration),
                signingCredentials: TokenOptions.SigningCredentials);
        }

        public string GetJwtString(ClaimsIdentity identity)
        {
            return new JwtSecurityTokenHandler().WriteToken(GetJwt(identity));
        }

        public ClaimsIdentity Login(string login, string password)
        {
            return login == "Administrator" && password == "Administrator" ?
                new ClaimsIdentity(new GenericIdentity(login, "Token")) :
                default(ClaimsIdentity);
        }

        public static long ToUnixEpochDate(DateTime date) => new DateTimeOffset(date).ToUniversalTime().ToUnixTimeSeconds();
    }
}