using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Server.Persistence;

namespace TickTrader.Algo.Server
{
    internal class PluginManager
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<PluginManager>();

        private readonly AlgoServerPrivate _server;
        private readonly Dictionary<string, IActorRef> _plugins = new Dictionary<string, IActorRef>();
        private readonly Dictionary<string, string> _pluginAccountMap = new Dictionary<string, string>();
        private readonly PluginIdHelper _pluginIdHelper = new PluginIdHelper();


        public PluginManager(AlgoServerPrivate server)
        {
            _server = server;
        }


        public async Task Shutdown()
        {
            _logger.Debug("Stopping...");

            await Task.WhenAll(_plugins.Select(p => ShutdownPluginInternal(p.Key, p.Value)));

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
                    startTasks.Add(plugin.Ask(PluginActor.StartCmd.Instance)
                        .OnException(ex => _logger.Error(ex, $"Failed to start plugin {id}")));
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

            return new PluginModel(CreatePluginIntenal(id, pluginSavedState));
        }

        public Task UpdateConfig(ChangePluginConfigRequest request)
        {
            var id = request.PluginId;
            if (!_plugins.TryGetValue(id, out var plugin))
                return Task.FromException(Errors.PluginNotFound(id));

            return plugin.Ask(new PluginActor.UpdateConfigCmd(request.NewConfig));
        }

        public Task RemovePlugin(RemovePluginRequest request) => RemovePluginInternal(request.PluginId);

        public Task StartPlugin(StartPluginRequest request)
        {
            var id = request.PluginId;
            if (!_plugins.TryGetValue(id, out var plugin))
                return Task.FromException(Errors.PluginNotFound(id));

            return plugin.Ask(PluginActor.StartCmd.Instance);
        }

        public Task StopPlugin(StopPluginRequest request)
        {
            var id = request.PluginId;
            if (!_plugins.TryGetValue(id, out var plugin))
                return Task.FromException(Errors.PluginNotFound(id));

            return plugin.Ask(PluginActor.StopCmd.Instance);
        }

        public Task<PluginLogRecord[]> GetPluginLogs(PluginLogsRequest request)
        {
            var id = request.PluginId;
            if (!_plugins.TryGetValue(id, out var plugin))
                return Task.FromException<PluginLogRecord[]>(Errors.PluginNotFound(id));

            return plugin.Ask<PluginLogRecord[]>(request);
        }

        public Task<string> GetPluginStatus(PluginStatusRequest request)
        {
            var id = request.PluginId;
            if (!_plugins.TryGetValue(id, out var plugin))
                return Task.FromException<string>(Errors.PluginNotFound(id));

            return plugin.Ask<string>(request);
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

        public async Task RemoveAllPluginsFromAccount(string accId)
        {
            await Task.WhenAll(_pluginAccountMap.Where(p => p.Value == accId)
                .Select(p => RemovePluginInternal(p.Key).OnException(ex => _logger.Error(ex, $"Failed to remove plugin {p.Key}"))));
        }

        public void TellAllPlugins(object msg)
        {
            foreach(var plugin in _plugins)
            {
                try
                {
                    plugin.Value.Tell(msg);
                }
                catch(Exception ex)
                {
                    _logger.Error(ex, $"Failed to send msg to plugin '{plugin.Key}'");
                }
            }
        }


        private IActorRef CreatePluginIntenal(string id, PluginSavedState savedState)
        {
            var plugin = PluginActor.Create(_server, savedState);
            _plugins[id] = plugin;
            _pluginAccountMap[id] = savedState.AccountId;
            return plugin;
        }

        private async Task RemovePluginInternal(string id)
        {
            if (!_plugins.TryGetValue(id, out var plugin))
                throw Errors.PluginNotFound(id);

            await _server.SavedState.RemovePlugin(id);

            _plugins.Remove(id);
            _pluginAccountMap.Remove(id);

            try
            {
                await ShutdownPluginInternal(id, plugin);
            }
            finally
            {
                _server.SendUpdate(PluginModelUpdate.Removed(id));
            }
        }

        private async Task ShutdownPluginInternal(string id, IActorRef plugin)
        {
            await plugin.Ask(PluginActor.StopCmd.Instance)
                .OnException(ex => _logger.Error(ex, $"Failed to stop plugin {id}"));

            await ActorSystem.StopActor(plugin)
                    .OnException(ex => _logger.Error(ex, $"Failed to stop actor {plugin.Name}"));
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
