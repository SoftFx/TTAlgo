using Microsoft.AspNetCore.Builder;
using System;
using System.Linq;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Extensions
{
    public static class SignalRAuthenticationExtensions
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
                            .Split('&').SingleOrDefault(x => x.Contains("authorization-token"))?.Split('=')[1];

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
