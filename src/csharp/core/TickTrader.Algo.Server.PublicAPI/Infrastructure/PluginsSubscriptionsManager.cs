using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using System.Linq;

namespace TickTrader.Algo.Server.PublicAPI
{
    internal sealed class PluginsSubscriptionsManager
    {
        private readonly Dictionary<string, Timestamp> _logsSubscriptions;
        private readonly HashSet<string> _statusSubscriptions;


        internal int LogsSubscriptionsCount => _logsSubscriptions.Count;


        internal PluginsSubscriptionsManager()
        {
            _logsSubscriptions = new Dictionary<string, Timestamp>();
            _statusSubscriptions = new HashSet<string>();
        }


        internal bool TryAddLogsSubscription(string pluginId, Timestamp lastTimePoint)
        {
            if (_logsSubscriptions.ContainsKey(pluginId))
                return false;

            _logsSubscriptions.Add(pluginId, lastTimePoint);

            return true;
        }


        internal bool TryUpdateLogsSubscriptionTime(string pluginId, Timestamp newTimePoint)
        {
            if (!_logsSubscriptions.ContainsKey(pluginId))
                return false;

            _logsSubscriptions[pluginId] = newTimePoint;

            return true;
        }


        internal bool TryRemoveLogsSubscription(string pluginId)
        {
            if (!_logsSubscriptions.ContainsKey(pluginId))
                return false;

            _logsSubscriptions.Remove(pluginId);

            return true;
        }

        internal List<PluginLogsSubscribeRequest> BuildLogsSubscriptionRequests()
        {
            var requests = new List<PluginLogsSubscribeRequest>(LogsSubscriptionsCount);

            foreach (var sub in _logsSubscriptions.ToList())
            {
                requests.Add(new PluginLogsSubscribeRequest
                {
                    PluginId = sub.Key,
                    LastLogTimeUtc = sub.Value,
                });
            }

            _logsSubscriptions.Clear();

            return requests;
        }
    }
}
