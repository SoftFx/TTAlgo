using System.Threading.Tasks;
using TickTrader.Algo.Async;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;

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


        public Task<AlertRecordInfo[]> GetAlerts(PluginAlertsRequest request) => _ref.Ask<AlertRecordInfo[]>(request);

        internal void SendPluginAlert(string pluginId, PluginLogRecord record) => _ref.Tell(new AlertManager.PluginAlertMsg(pluginId, record));

        internal void SendServerAlert(string message) => _ref.Tell(new AlertManager.ServerAlertMsg(message));
    }
}
