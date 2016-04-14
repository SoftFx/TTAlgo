using System;
using System.Collections.Generic;
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
    }
}
