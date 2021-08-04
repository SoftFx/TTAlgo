using System.Collections.Generic;
using System.Linq;

namespace TickTrader.Algo.Server.PublicAPI
{
    internal sealed class PluginsSubscriptionsManager
    {
        private readonly HashSet<string> _logsSubscriptions;
        private readonly HashSet<string> _statusSubscriptions;


        internal int LogsSubscriptionsCount => _logsSubscriptions.Count;


        internal PluginsSubscriptionsManager()
        {
            _logsSubscriptions = new HashSet<string>();
            _statusSubscriptions = new HashSet<string>();
        }


        internal bool TryAddLogsSubscription(string pluginId)
        {
            if (_logsSubscriptions.Contains(pluginId))
                return false;

            _logsSubscriptions.Add(pluginId);

            return true;
        }

        internal bool TryRemoveLogsSubscription(string pluginId)
        {
            if (!_logsSubscriptions.Contains(pluginId))
                return false;

            _logsSubscriptions.Remove(pluginId);

            return true;
        }

        internal List<PluginLogsSubscribeRequest> BuildLogsSubscriptionRequests()
        {
            var requests = new List<PluginLogsSubscribeRequest>(LogsSubscriptionsCount);

            foreach (var pluginId in _logsSubscriptions.ToList())
            {
                requests.Add(new PluginLogsSubscribeRequest
                {
                    PluginId = pluginId,
                });
            }

            _logsSubscriptions.Clear();

            return requests;
        }
    }
}
