using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using System.Windows.Threading;

namespace TickTrader.BotTerminal.Lib
{
    public static class DataflowHelper
    {
        private static UiTaskScheduler dispatcherScheduler = new UiTaskScheduler(App.Current.Dispatcher, DispatcherPriority.Input);

        public static ActionBlock<T> CreateUiActionBlock<T>(Action<T> action, int queueSize, int msgPerTask, CancellationToken cToken)
        {
            var options = new ExecutionDataflowBlockOptions()
            {
                TaskScheduler = dispatcherScheduler,
                MaxDegreeOfParallelism = 1,
                BoundedCapacity = queueSize,
                MaxMessagesPerTask = msgPerTask,
                CancellationToken = cToken
            };

            return new ActionBlock<T>(action, options);
        }

        public static BatchBlock<T> CreateBatchBlock<T>(int batchSize, int queueSize, CancellationToken cToken)
        {
            var options = new GroupingDataflowBlockOptions()
            {
                TaskScheduler = dispatcherScheduler,
                BoundedCapacity = queueSize,
                CancellationToken = cToken
            };

            return new BatchBlock<T>(batchSize, options);
        }

        public static IDisposable AppendLinkWithCompletion<TOutput>(this ISourceBlock<TOutput> source, ITargetBlock<TOutput> target)
        {
            return source.LinkTo(target, new DataflowLinkOptions() { PropagateCompletion = true, Append = true });
        }

        public static IDisposable PrependLinkWithCompletion<TOutput>(this ISourceBlock<TOutput> source, ITargetBlock<TOutput> target)
        {
            return source.LinkTo(target, new DataflowLinkOptions() { PropagateCompletion = true, Append = false });
        }
    }
}
