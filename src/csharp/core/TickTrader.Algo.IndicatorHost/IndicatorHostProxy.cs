using System.Threading.Channels;
using System.Threading.Tasks;
using TickTrader.Algo.Async;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Server;

namespace TickTrader.Algo.IndicatorHost
{
    public class IndicatorHostProxy
    {
        private readonly ChannelEventSource<PackageUpdate> _pkgUpdateEventSrc = new();
        private readonly ChannelEventSource<PackageStateUpdate> _pkgStateUpdateEventSrc = new();
        private readonly ChannelEventSource<AlertRecordInfo> _alertEventSrc = new();

        private IActorRef _algoHost, _indicatorHost;
        private bool _ownsAlgoHost;
        private Channel<object> _downlink;


        public IEventSource<PackageUpdate> OnPkgUpdated => _pkgUpdateEventSrc;

        public IEventSource<PackageStateUpdate> OnPkgStateUpdated => _pkgStateUpdateEventSrc;

        public IEventSource<AlertRecordInfo> OnAlert => _alertEventSrc;


        public IndicatorHostProxy() { }


        public async Task Init(IndicatorHostSettings settings, IActorRef algoHost = null)
        {
            _algoHost = algoHost;
            if (algoHost == null)
            {
                _ownsAlgoHost = true;
                settings.HostSettings.RuntimeSettings.WorkingDirectory ??= settings.DataFolder;
                _algoHost = AlgoHostActor.Create(settings.HostSettings);
            }

            _indicatorHost = IndicatorHostActor.Create(_algoHost, settings);
            await AlgoHostModel.AddConsumer(_algoHost, _indicatorHost);

            _downlink = DefaultChannelFactory.CreateForOneToOne<object>();
            await _indicatorHost.Ask(new AttachDownlinkCmd(_downlink.Writer));
            _ = Task.Run(() => _downlink.Consume(ProcessMsg));

            if (_ownsAlgoHost)
                await AlgoHostModel.Start(_algoHost);
        }

        public async Task Shutdown()
        {
            _downlink.Writer.TryComplete();
            await IndicatorHostModel.Shutdown(_indicatorHost);
            if (_ownsAlgoHost)
                await AlgoHostModel.Stop(_algoHost);
        }

        public Task Start() => IndicatorHostModel.Start(_indicatorHost);

        public Task Stop() => IndicatorHostModel.Stop(_indicatorHost);

        public Task SetAccountProxy(IAccountProxy accProxy) => IndicatorHostModel.SetAccountProxy(_indicatorHost, accProxy);

        public Task<ChartHostProxy> CreateChart(string symbol, Feed.Types.Timeframe timeframe, Feed.Types.MarketSide marketSide,
            int barsCount = ChartBuilderActor.DefaultBarsCount) => IndicatorHostModel.CreateChart(_indicatorHost, symbol, timeframe, marketSide, barsCount);


        private void ProcessMsg(object msg)
        {
            switch (msg)
            {
                case PackageUpdate pkgUpdate: _pkgUpdateEventSrc.Send(pkgUpdate); break;
                case PackageStateUpdate stateUpdate: _pkgStateUpdateEventSrc.Send(stateUpdate); break;
                case AlertRecordInfo alertMsg: _alertEventSrc.Send(alertMsg); break;
            }
        }


        internal record AttachDownlinkCmd(ChannelWriter<object> Sink);
    }
}
