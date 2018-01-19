using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ActorSharp.Sample
{
    public class Producer : Actor
    {
        private async Task<int> InvokeSum(int from, int to, int pageSize)
        {
            var consumer = new Consumer.Handler();
            var channel = await consumer.BeginAggregate(pageSize);

            for (int i = from; i <= to; i++)
                await channel.Write(i);

            await channel.Close();

            return await consumer.GetResult();
        }

        public class Handler : ControlHandler<Producer>
        {
            public Task<int> InvokeSum(int from, int to, int pageSize)
            {
                return CallActor(a => a.InvokeSum(from, to, pageSize));
            }
        }
    }
}
