using System.Threading.Tasks;

namespace TickTrader.DedicatedServer.Extensions
{
    public static class TaskExtensions
    {
        public static readonly Task CompletedTask = Task.FromResult(false);
    }
}
