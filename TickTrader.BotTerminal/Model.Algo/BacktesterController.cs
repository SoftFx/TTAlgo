using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using TickTrader.Algo.Async;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    public class BacktesterController : IDisposable
    {
        private readonly IActorRef _actor;
        private readonly ChannelEventSource<BacktesterProgressUpdate> _progressEventSrc = new ChannelEventSource<BacktesterProgressUpdate>();


        public IEventSource<BacktesterProgressUpdate> OnProgressUpdate => _progressEventSrc;


        public BacktesterController(IActorRef actor)
        {
            _actor = actor;
        }


        public void Dispose()
        {
            _progressEventSrc.Dispose();
        }


        internal async Task Init()
        {
            await _actor.Ask(new SubscribeToProgressUpdatesCmd(_progressEventSrc.Writer));
        }


        public Task Start(string configPath) => _actor.Ask(new StartBacktesterRequest { ConfigPath = configPath });

        public Task Stop() => _actor.Ask(new StopBacktesterRequest());

        public Task AwaitStop() => _actor.Ask(AwaitStopRequest.Instance);


        internal class AwaitStopRequest : Singleton<AwaitStopRequest> { }

        internal class SubscribeToProgressUpdatesCmd
        {
            public ChannelWriter<BacktesterProgressUpdate> Sink { get; }

            public SubscribeToProgressUpdatesCmd(ChannelWriter<BacktesterProgressUpdate> sink)
            {
                Sink = sink;
            }
        }
    }
}
