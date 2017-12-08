using System.Threading.Tasks;

namespace TickTrader.BotAgent.Extensions
{
    public static class TaskExtensions
    {
        public static readonly Task CompletedTask = Task.FromResult(false);

        public static void Forget(this Task task)
        {
        }
    }
}
