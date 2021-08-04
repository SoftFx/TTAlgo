using System.Collections.Generic;
using System.Linq;

namespace TickTrader.Algo.Server.PublicAPI
{
    internal sealed class PluginsSubscriptionsManager
    {
        private readonly HashSet<string> _logsSubscriptions;
        private readonly HashSet<string> _statusSubscriptions;


        internal int LogsSubscriptionsCount => _logsSubscriptions.Count;

        internal int StatusSubscriptionsCount => _statusSubscriptions.Count;


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


        internal bool TryAddStatusSubscription(string pluginId)
        {
            if (_statusSubscriptions.Contains(pluginId))
                return false;

            _statusSubscriptions.Add(pluginId);

            return true;
        }

        internal bool TryRemoveStatusSubscription(string pluginId)
        {
            if (!_statusSubscriptions.Contains(pluginId))
                return false;

            _statusSubscriptions.Remove(pluginId);

            return true;
        }

        internal List<PluginStatusSubscribeRequest> BuildStatusSubscriptionRequests()
        {
            var requests = new List<PluginStatusSubscribeRequest>(StatusSubscriptionsCount);

            foreach (var pluginId in _statusSubscriptions.ToList())
            {
                requests.Add(new PluginStatusSubscribeRequest
                {
                    PluginId = pluginId,
                });
            }

            _statusSubscriptions.Clear();

            return requests;
        }
    }
}
