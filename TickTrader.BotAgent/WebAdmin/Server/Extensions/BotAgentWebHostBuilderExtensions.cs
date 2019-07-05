using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TickTrader.Algo.Protocol;
using TickTrader.Algo.Protocol.Grpc;
using TickTrader.BotAgent.BA;
using TickTrader.BotAgent.BA.Models;
using TickTrader.BotAgent.WebAdmin.Server.HostedServices;
using TickTrader.BotAgent.WebAdmin.Server.Protocol;

namespace TickTrader.BotAgent.WebAdmin.Server.Extensions
{
    public static class BotAgentWebHostBuilderExtensions
    {
        public static IWebHostBuilder AddBotAgent(this IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IBotAgent>(s => new ServerModel.Handler(ServerModel.Load()));
                services.AddHostedService<BotAgentHostedService>();
            });
            return builder;
        }

        public static IWebHostBuilder AddProtocolServer(this IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IServerSettings>(s => s.GetRequiredService<IConfiguration>().GetProtocolServerSettings());
                services.AddSingleton<IJwtProvider, JwtProviderAdapter>();
                services.AddSingleton<IBotAgentServer, BotAgentServerAdapter>();
                services.AddSingleton<IProtocolServer, GrpcServer>();
                services.AddHostedService<ProtocolHostedService>();
            });
            return builder;
        }
    }
}
