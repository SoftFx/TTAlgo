using System.Threading.Tasks;
using TickTrader.BotAgent.WebAdmin.Server.Models;

namespace TickTrader.BotAgent.BA
{
    public interface IBotAgent
    {
        Task InitAsync(FdkSettings fdkSettings);

        Task ShutdownAsync();
    }
}
