using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Server.Persistence;

namespace TickTrader.Algo.Server
{
    internal class PluginManager
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<RuntimeManager>();

        private readonly AlgoServerPrivate _server;
        private readonly Dictionary<string, PluginModel> _plugins = new Dictionary<string, PluginModel>();
        private readonly PluginIdHelper _pluginIdHelper = new PluginIdHelper();


        public PluginManager(AlgoServerPrivate server)
        {
            _server = server;
        }


        public async Task Shutdown()
        {
            _logger.Debug("Stopping...");

            await Task.WhenAll(_plugins.Values.Select(p =>
                p.Stop().OnException(ex => _logger.Error(ex, $"Failed to stop plugin {p.Id}"))).ToArray());

            _logger.Debug("Stopped");
        }

        public async Task Restore(ServerSavedState savedState)
        {
            _logger.Debug("Restoring saved state...");

            var startTasks = new List<Task>(savedState.Plugins.Count);
            foreach (var p in savedState.Plugins)
            {
                var id = p.Key;
                var plugin = CreatePluginIntenal(id, p.Value);
                if (p.Value.IsRunning)
                    startTasks.Add(plugin.Start().OnException(ex => _logger.Error(ex, $"Failed to stop plugin {id}")));
            }

            await Task.WhenAll(startTasks);

            _logger.Debug("Restored saved state");
        }

        public async Task<PluginModel> AddPlugin(AddPluginRequest request)
        {
            var id = request.Config.InstanceId;
            if (_plugins.ContainsKey(id))
                throw Errors.DuplicatePlugin(id);

            var pluginSavedState = new PluginSavedState
            {
                Id = id,
                AccountId = request.AccountId,
            };
            pluginSavedState.PackConfig(request.Config);

            await _server.SavedState.AddPlugin(pluginSavedState);

            return CreatePluginIntenal(id, pluginSavedState);
        }

        public Task UpdateConfig(ChangePluginConfigRequest request)
        {
            var id = request.PluginId;
            if (!_plugins.TryGetValue(id, out var plugin))
                return Task.FromException(Errors.PluginNotFound(id));

            return plugin.UpdateConfig(request.NewConfig);
        }

        public async Task RemovePlugin(RemovePluginRequest request)
        {
            var id = request.PluginId;
            if (!_plugins.TryGetValue(id, out var plugin))
                throw Errors.PluginNotFound(id);

            await _server.SavedState.RemovePlugin(id);

            _plugins.Remove(id);

            _server.SendUpdate(PluginModelUpdate.Removed(id));

            await plugin.Stop().OnException(ex => _logger.Error(ex, $"Failed to stop plugin {id}"));
        }

        public Task StartPlugin(StartPluginRequest request)
        {
            var id = request.PluginId;
            if (!_plugins.TryGetValue(id, out var plugin))
                return Task.FromException(Errors.PluginNotFound(id));

            return plugin.Start();
        }

        public Task StopPlugin(StopPluginRequest request)
        {
            var id = request.PluginId;
            if (!_plugins.TryGetValue(id, out var plugin))
                return Task.FromException(Errors.PluginNotFound(id));

            return plugin.Stop();
        }

        public Task<PluginLogRecord[]> GetPluginLogs(PluginLogsRequest request)
        {
            var id = request.PluginId;
            if (!_plugins.TryGetValue(id, out var plugin))
                return Task.FromException<PluginLogRecord[]>(Errors.PluginNotFound(id));

            return plugin.GetLogs(request);
        }

        public Task<string> GetPluginStatus(PluginStatusRequest request)
        {
            var id = request.PluginId;
            if (!_plugins.TryGetValue(id, out var plugin))
                return Task.FromException<string>(Errors.PluginNotFound(id));

            return plugin.GetStatus(request);
        }

        public bool PluginExists(string pluginId)
        {
            return _plugins.ContainsKey(pluginId);
        }

        public string GeneratePluginId(string pluginDisplayName)
        {
            int seed = 1;

            while (true)
            {
                var botId = _pluginIdHelper.BuildId(pluginDisplayName, seed.ToString());
                if (!_plugins.ContainsKey(botId))
                    return botId;

                seed++;
            }
        }


        private PluginModel CreatePluginIntenal(string id, PluginSavedState savedState)
        {
            var plugin = new PluginModel(PluginActor.Create(_server, savedState));
            _plugins[id] = plugin;
            return plugin;
        }


        internal class PluginExistsRequest
        {
            public string PluginId { get; }

            public PluginExistsRequest(string pluginId)
            {
                PluginId = pluginId;
            }
        }
    }
}
