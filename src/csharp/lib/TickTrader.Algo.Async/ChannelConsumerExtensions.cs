using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TickTrader.Algo.Async
{
    public static class ChannelConsumerExtensions
    {
        public static Task Consume<T>(this Channel<T> channel, Func<T, Task> itemAction, int batchSize = 1, int workersCnt = 1, CancellationToken cancelToken = default)
        {
            if (itemAction == null)
                throw new ArgumentNullException(nameof(itemAction));

            if (workersCnt == 1)
                return ProcessItems(channel.Reader, itemAction, batchSize, cancelToken);

            var workers = new Task[workersCnt];
            for (var i = 0; i < workers.Length; i++)
                workers[i] = ProcessItems(channel.Reader, itemAction, batchSize, cancelToken);

            return Task.WhenAll(workers);
        }

        public static Task Consume<T>(this Channel<T> channel, Action<T> itemAction, int batchSize = 1, int workersCnt = 1, CancellationToken cancelToken = default)
        {
            if (itemAction == null)
                throw new ArgumentNullException(nameof(itemAction));

            if (workersCnt == 1)
                return ProcessItems(channel.Reader, itemAction, batchSize, cancelToken);

            var workers = new Task[workersCnt];
            for (var i = 0; i < workers.Length; i++)
                workers[i] = ProcessItems(channel.Reader, itemAction, batchSize, cancelToken);

            return Task.WhenAll(workers);
        }


        private static async Task ProcessItems<T>(ChannelReader<T> reader, Func<T, Task> itemAction, int batchSize, CancellationToken cancelToken)
        {
            await Task.Yield();

            while (!cancelToken.IsCancellationRequested && await reader.WaitToReadAsync())
            {
                var i = 0;
                for (; i < batchSize; i++)
                {
                    if (!reader.TryRead(out var item))
                        break;

                    try
                    {
                        await itemAction.Invoke(item);
                    }
                    catch (Exception) { }
                }

                await Task.Yield(); // break sync processing
            }
        }

        private static async Task ProcessItems<T>(ChannelReader<T> reader, Action<T> itemAction, int batchSize, CancellationToken cancelToken)
        {
            await Task.Yield();

            while (!cancelToken.IsCancellationRequested && await reader.WaitToReadAsync())
            {
                var i = 0;
                for (; i < batchSize; i++)
                {
                    if (!reader.TryRead(out var item))
                        break;

                    try
                    {
                        itemAction.Invoke(item);
                    }
                    catch (Exception) { }
                }

                await Task.Yield(); // break sync processing
            }
        }
    }
}
