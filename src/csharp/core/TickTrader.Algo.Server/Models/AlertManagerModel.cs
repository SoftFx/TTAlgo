using System.Threading.Channels;
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


        public AlertManagerModel(IActorRef actor)
        {
            _ref = actor;
        }


        public Task<AlertRecordInfo[]> GetAlerts(PluginAlertsRequest request) => _ref.Ask<AlertRecordInfo[]>(request);

        internal void SendPluginAlert(string pluginId, PluginLogRecord record) => _ref.Tell(new AlertManager.PluginAlertMsg(pluginId, record));

        internal void SendServerAlert(string message) => _ref.Tell(new AlertManager.ServerAlertMsg(message));

        internal void AttachAlertChannel(ChannelWriter<AlertRecordInfo> sink) => _ref.Ask(new AlertManager.AttachAlertChannelCmd(sink, null));
    }
}
