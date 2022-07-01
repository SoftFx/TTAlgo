using System.Collections.Generic;
using System.Linq;

namespace TickTrader.Algo.Server.PublicAPI
{
    public sealed class PluginsSubscriptionsManager
    {
        private readonly HashSet<string> _logsSubscriptions;
        private readonly HashSet<string> _statusSubscriptions;

        private readonly object _syncLock = new object();


        internal int LogsSubscriptionsCount => _logsSubscriptions.Count;

        internal int StatusSubscriptionsCount => _statusSubscriptions.Count;


        public PluginsSubscriptionsManager()
        {
            _logsSubscriptions = new HashSet<string>();
            _statusSubscriptions = new HashSet<string>();
        }


        public bool TryAddLogsSubscription(string pluginId)
        {
            lock (_syncLock)
            {
                if (_logsSubscriptions.Contains(pluginId))
                    return false;

                _logsSubscriptions.Add(pluginId);

                return true;
            }
        }

        public bool TryRemoveLogsSubscription(string pluginId)
        {
            lock (_syncLock)
            {
                if (!_logsSubscriptions.Contains(pluginId))
                    return false;

                _logsSubscriptions.Remove(pluginId);

                return true;
            }
        }

        public List<PluginLogsSubscribeRequest> BuildLogsSubscriptionRequests()
        {
            lock (_syncLock)
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


        public bool TryAddStatusSubscription(string pluginId)
        {
            lock (_syncLock)
            {
                if (_statusSubscriptions.Contains(pluginId))
                    return false;

                _statusSubscriptions.Add(pluginId);

                return true;
            }
        }

        public bool TryRemoveStatusSubscription(string pluginId)
        {
            lock (_syncLock)
            {
                if (!_statusSubscriptions.Contains(pluginId))
                    return false;

                _statusSubscriptions.Remove(pluginId);

                return true;
            }        }

        public List<PluginStatusSubscribeRequest> BuildStatusSubscriptionRequests()
        {
            lock (_syncLock)
            {
                var requests = new List<PluginStatusSubscribeRequest>(StatusSubscriptionsCount);

                foreach (var pluginId in _statusSubscriptions.ToList())
                {
                    requests.Add(new PluginStatusSubscribeRequest { PluginId = pluginId });
                }

                _statusSubscriptions.Clear();

                return requests;
            }        }
    }
}