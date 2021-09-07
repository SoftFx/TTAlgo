using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace TickTrader.BotAgent.BA
{
    public interface IBotAgent
    {
        Task InitAsync(IConfiguration config);

        Task ShutdownAsync();
    }
}
