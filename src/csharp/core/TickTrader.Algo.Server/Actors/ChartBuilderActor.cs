using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Server
{
    internal class ChartBuilderActor : Actor
    {
        private readonly int _id;
        private readonly string _symbol;
        private readonly Feed.Types.Timeframe _timeframe;
        private readonly Feed.Types.MarketSide _marketSide;


        public ChartBuilderActor(ChartInfo info)
        {
            _id = info.Id;
            _symbol = info.Symbol;
            _timeframe = info.Timeframe;
            _marketSide = info.MarketSide;

            Receive<ChartBuilderModel.StartCmd>(Start);
            Receive<ChartBuilderModel.StopCmd>(Stop);
            Receive<ChartHostProxy.AddIndicatorRequest>(AddIndicator);
            Receive<ChartHostProxy.RemoveIndicatorRequest>(RemoveIndicator);
        }


        public static IActorRef Create(ChartInfo info)
        {
            return ActorSystem.SpawnLocal(() => new ChartBuilderActor(info), $"{nameof(ChartBuilderActor)} {info.Id}");
        }


        private void Start(ChartBuilderModel.StartCmd cmd)
        {

        }

        private void Stop(ChartBuilderModel.StopCmd cmd)
        {

        }

        private void AddIndicator(ChartHostProxy.AddIndicatorRequest request)
        {

        }

        private void RemoveIndicator(ChartHostProxy.RemoveIndicatorRequest request)
        {

        }
    }
}
