using System.Threading.Tasks;

namespace TickTrader.DedicatedServer.Server.DS
{
    public static class TaskExtensions
    {
        public static readonly Task CompletedTask = Task.FromResult(false);
    }
}
