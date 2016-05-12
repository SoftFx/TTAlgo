using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TickTrader.Algo.Core.Lib
{
    public static class Dataflow
    {
        public static async void BatchLinkTo<T>(this IReceivableSourceBlock<T> input, ITargetBlock<T[]> output, int batchSize)
        {
            List<T> buffer = new List<T>(batchSize);

            try
            {
                while (true)
                {
                    buffer.Add(await input.ReceiveAsync());

                    T item;
                    while (buffer.Count < batchSize)
                    {
                        if (!input.TryReceive(out item))
                            break;
                        buffer.Add(item);
                    }

                    await output.SendAsync(buffer.ToArray());

                    buffer.Clear();
                }
            }
            catch (InvalidOperationException) { /* normal exit */ }
        }

        private static readonly ExecutionDataflowBlockOptions defaultForkOptions = new ExecutionDataflowBlockOptions() { };
        private static readonly DataflowLinkOptions forkInnerLinkOptions = new DataflowLinkOptions() { PropagateCompletion = true };

        public static IPropagatorBlock<T, T> CreatePriorityFork<T>(Func<T, bool> prioritySelector)
        {
            return CreatePriorityFork(prioritySelector, defaultForkOptions);
        }

        public static IPropagatorBlock<T, T> CreatePriorityFork<T>(Func<T, bool> prioritySelector, ExecutionDataflowBlockOptions options)
        {
            var targeOptions = new ExecutionDataflowBlockOptions() { BoundedCapacity = options.BoundedCapacity, SingleProducerConstrained = options.SingleProducerConstrained, TaskScheduler = options.TaskScheduler };
            var srcOptions = new ExecutionDataflowBlockOptions() { BoundedCapacity = options.BoundedCapacity, SingleProducerConstrained = true, TaskScheduler = options.TaskScheduler };

            ConcurrentQueue<T> priorityQueue = new ConcurrentQueue<T>();
            ConcurrentQueue<T> normalQueue = new ConcurrentQueue<T>();

            var target = new TransformBlock<T, int>(i =>
            {
                if (prioritySelector(i))
                    priorityQueue.Enqueue(i);
                else
                    normalQueue.Enqueue(i);
                return 0;
            }, targeOptions);

            var src = new TransformBlock<int, T>(i =>
            {
                T result;
                bool assertCheck;
                if (priorityQueue.Count > 0)
                    assertCheck = priorityQueue.TryDequeue(out result);
                else
                    assertCheck = normalQueue.TryDequeue(out result);
                Debug.Assert(assertCheck, "Priority queue unexpected collision!");
                return result;
            }, srcOptions);

            target.LinkTo(src, forkInnerLinkOptions);

            return DataflowBlock.Encapsulate(target, src);
        }
    }
}
