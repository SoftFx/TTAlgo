using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ActorSharp.Sample
{
    public class Consumer : Actor
    {
        private int _sum;

        private async void AggregateLoop(IRxChannel<int> stream)
        {
            while (await stream)
                _sum += 1;
        }

        public class Handler : ControlHandler<Consumer>
        {
            public async Task<ITxChannel<int>> BeginAggregate(int pageSize)
            {
                var channel = NewTxChannel<int>();
                await CallActor(a => a.AggregateLoop(Marshal(channel, pageSize)));
                return channel;
            }

            public Task<int> GetResult()
            {
                return CallActor(a => a._sum);
            }
        }
    }

}
