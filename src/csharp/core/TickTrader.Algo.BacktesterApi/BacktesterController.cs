using System;
using System.IO;
using System.Threading.Channels;
using System.Threading.Tasks;
using TickTrader.Algo.Async;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.BacktesterApi
{
    public class BacktesterController : IDisposable
    {
        private readonly IActorRef _actor;
        private readonly string _resultsDir;
        private readonly ChannelEventSource<BacktesterProgressUpdate> _progressEventSrc = new ChannelEventSource<BacktesterProgressUpdate>();
        private readonly ChannelEventSource<BacktesterStateUpdate> _stateEventSrc = new ChannelEventSource<BacktesterStateUpdate>();


        public IEventSource<BacktesterProgressUpdate> OnProgressUpdate => _progressEventSrc;

        public IEventSource<BacktesterStateUpdate> OnStateUpdate => _stateEventSrc;


        public BacktesterController(IActorRef actor, string resultsDir)
        {
            _actor = actor;
            _resultsDir = resultsDir;
        }


        public void Dispose()
        {
            _progressEventSrc.Dispose();
            _actor.Tell(BacktesterControlActor.DeinitCmd.Instance);
        }


        internal async Task Init()
        {
            await _actor.Ask(new SubscribeToProgressUpdatesCmd(_progressEventSrc.Writer));
        }


        public Task Start() => _actor.Ask(new StartBacktesterRequest());

        public Task Stop() => _actor.Ask(new StopBacktesterRequest());

        public Task AwaitStop() => _actor.Ask(AwaitStopRequest.Instance);

        public string GetResultsPath()
        {
            var zipPath = _resultsDir + ".zip";
            if (File.Exists(zipPath))
                return zipPath;

            return _resultsDir;

        }


        internal class AwaitStopRequest : Singleton<AwaitStopRequest> { }

        internal class SubscribeToProgressUpdatesCmd
        {
            public ChannelWriter<BacktesterProgressUpdate> Sink { get; }

            public SubscribeToProgressUpdatesCmd(ChannelWriter<BacktesterProgressUpdate> sink)
            {
                Sink = sink;
            }
        }

        internal class SubscribeToStateUpdatesCmd
        {
            public ChannelWriter<BacktesterStateUpdate> Sink { get; }

            public SubscribeToStateUpdatesCmd(ChannelWriter<BacktesterStateUpdate> sink)
            {
                Sink = sink;
            }
        }
    }
}
