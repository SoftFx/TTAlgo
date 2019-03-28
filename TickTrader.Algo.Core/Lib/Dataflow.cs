using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TickTrader.Algo.Core.Lib
{
    public static class Dataflow
    {
        public static async Task BatchLinkTo<T>(this IReceivableSourceBlock<T> input, ITargetBlock<T[]> output, int batchSize)
        {
            var buffer = new List<T>(batchSize);

            while (await input.OutputAvailableAsync().ConfigureAwait(false))
            {
                while (input.TryReceive(out var item))
                {
                    buffer.Add(item);
                    if (buffer.Count >= batchSize)
                    {
                        await output.SendAsync(buffer.ToArray()).ConfigureAwait(false);
                        buffer.Clear();
                    }
                }

                if (buffer.Count > 0)
                {
                    await output.SendAsync(buffer.ToArray()).ConfigureAwait(false);
                    buffer.Clear();
                }
            }
        }

        public static IPropagatorBlock<T, T[]> CreateBatchingBlock<T>(int batchSize)
        {
            var inputBufferOptions = new DataflowBlockOptions() { BoundedCapacity = batchSize };
            var outputBufferOptions = new DataflowBlockOptions() { BoundedCapacity = 1 };

            var inBuffer = new BufferBlock<T>(inputBufferOptions);
            var outBuffer = new BufferBlock<T[]>(outputBufferOptions);

            inBuffer.BatchLinkTo(outBuffer, batchSize).Forget();

            return DataflowBlock.Encapsulate<T, T[]>(inBuffer, outBuffer);
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
