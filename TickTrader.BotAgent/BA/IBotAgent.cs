using System.Threading.Tasks;
using TickTrader.Algo.Account;

namespace TickTrader.BotAgent.BA
{
    public interface IBotAgent
    {
        Task InitAsync(IFdkOptionsProvider fdkOptionsProvider);

        Task ShutdownAsync();
    }

    public interface IFdkOptionsProvider
    {
        ConnectionOptions GetConnectionOptions();
    }
}
