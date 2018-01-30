using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ActorSharp.Sample
{
    public class Consumer : Actor
    {
        private int _sum;

        private async void AggregateLoop(Channel<int> stream)
        {
            while (await stream.ReadNext())
                _sum += 1;
        }

        public class Handler : Handler<Consumer>
        {
            public Handler() : base(SpawnLocal<Consumer>())
            {
            }

            public async Task<Channel<int>> BeginAggregate(int pageSize)
            {
                var ch = Channel.NewInput<int>(pageSize);
                await Actor.OpenChannel(ch, (a, c) => a.AggregateLoop(c));
                return ch;
            }

            public Task<int> GetResult()
            {
                return Actor.Call(a => a._sum);
            }
        }
    }

}
