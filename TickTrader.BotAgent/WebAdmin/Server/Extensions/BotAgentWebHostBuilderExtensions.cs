using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TickTrader.Algo.Protocol;
using TickTrader.BotAgent.BA;

namespace TickTrader.BotAgent.WebAdmin.Server.Extensions
{
    public static class BotAgentWebHostBuilderExtensions
    {
        public static IWebHostBuilder AddBotAgent(this IWebHostBuilder builder, IBotAgent agent)
        {
            builder.ConfigureServices(s => s.AddSingleton(agent));
            return builder;
        }

        public static IWebHostBuilder AddProtocolServer(this IWebHostBuilder builder, ProtocolServer server)
        {
            builder.ConfigureServices(s => s.AddSingleton(server));
            return builder;
        }
    }
}
