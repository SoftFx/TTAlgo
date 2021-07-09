using TickTrader.Algo.Async;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Server
{
    public class AlertManagerModel
    {
        private readonly IActorRef _ref;
        private readonly ChannelEventSource<AlertRecordInfo> _alertEventSrc = new ChannelEventSource<AlertRecordInfo>();


        public IEventSource<AlertRecordInfo> AlertUpdated => _alertEventSrc;


        public AlertManagerModel(IActorRef actor)
        {
            _ref = actor;

            _ref.Tell(new AlertManager.AttachAlertChannelCmd(_alertEventSrc.Writer, null));
        }


        public void Dispose()
        {
            _alertEventSrc.Dispose();
        }
    }
}
