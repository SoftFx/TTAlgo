using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;

namespace TickTrader.Algo.Async
{
    public static class ParallelTaskScheduler
    {
        public static TaskScheduler ShortRunning { get; }

        public static TaskScheduler LongRunning { get; }


        static ParallelTaskScheduler()
        {
            var shortConcurrencyLevel = Math.Max(Environment.ProcessorCount / 2, 2);
            var longConcurrencyLevel = Math.Max(1, Math.Min(Environment.ProcessorCount / 2, 2));

            ShortRunning = new LimitedConcurrencyLevelTaskScheduler(shortConcurrencyLevel);
            LongRunning = new LimitedConcurrencyLevelTaskScheduler(longConcurrencyLevel);
        }
    }
}
