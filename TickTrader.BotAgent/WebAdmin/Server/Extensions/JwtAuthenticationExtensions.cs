using Microsoft.AspNetCore.Builder;
using System;
using System.Linq;

namespace TickTrader.BotAgent.WebAdmin.Server.Extensions
{
    public static class JwtAuthenticationExtensions
    {
        public static void UseJwtAuthentication(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                if (string.IsNullOrWhiteSpace(context.Request.Headers["Authorization"]))
                {
                    if (context.Request.QueryString.HasValue)
                    {
                        var token = context.Request.QueryString.Value
                            .Split('&').SingleOrDefault(x => x.Contains("access_token"))?.Split('=')[1];

                        if (!string.IsNullOrWhiteSpace(token))
                        {
                            context.Request.Headers.Add("Authorization", $"Bearer {token}");
                        }
                    }
                }
                await next.Invoke();
            });
        }
    }
}
