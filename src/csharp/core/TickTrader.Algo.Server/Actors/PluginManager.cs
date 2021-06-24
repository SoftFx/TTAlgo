using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Server.Persistence;

namespace TickTrader.Algo.Server
{
    internal class PluginManager : Actor
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<RuntimeManager>();

        private readonly Dictionary<string, PluginModel> _plugins = new Dictionary<string, PluginModel>();
        private readonly AlgoServer _server;


        public PluginManager(AlgoServer server)
        {
            _server = server;

            Receive<ShutdownCmd>(Shutdown);
            Receive<RestoreCmd>(Restore);
            Receive<AddPluginRequest>(AddPlugin);
            Receive<ChangePluginConfigRequest>(UpdateConfig);
            Receive<RemovePluginRequest>(RemovePlugin);
            Receive<StartPluginRequest>(StartPlugin);
            Receive<StopPluginRequest>(StopPlugin);
        }


        public static IActorRef Create(AlgoServer server)
        {
            return ActorSystem.SpawnLocal(() => new PluginManager(server), nameof(PluginManager));
        }


        private async Task Shutdown(ShutdownCmd cmd)
        {
            _logger.Debug("Stopping...");

            await Task.WhenAll(_plugins.Values.Select(p =>
                p.Stop().OnException(ex => _logger.Error(ex, $"Failed to stop plugin {p.Id}"))).ToArray());

            _logger.Debug("Stopped");
        }

        private async Task Restore(RestoreCmd cmd)
        {
            _logger.Debug("Restoring saved state...");

            var savedState = await _server.SavedState.GetSnapshot();
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

        private async Task<PluginModel> AddPlugin(AddPluginRequest request)
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

            var plugin = new PluginModel(PluginActor.Create(_server, pluginSavedState));
            _plugins[id] = plugin;

            return CreatePluginIntenal(id, pluginSavedState);
        }

        private Task UpdateConfig(ChangePluginConfigRequest request)
        {
            var id = request.PluginId;
            if (!_plugins.TryGetValue(id, out var plugin))
                return Task.FromException(Errors.PluginNotFound(id));

            return plugin.UpdateConfig(request.NewConfig);
        }

        private async Task RemovePlugin(RemovePluginRequest request)
        {
            var id = request.PluginId;
            if (!_plugins.TryGetValue(id, out var plugin))
                throw Errors.PluginNotFound(id);

            await _server.SavedState.RemovePlugin(id);

            _plugins.Remove(id);
            await plugin.Stop().OnException(ex => _logger.Error(ex, $"Failed to stop plugin {id}"));
        }

        private Task StartPlugin(StartPluginRequest request)
        {
            var id = request.PluginId;
            if (!_plugins.TryGetValue(id, out var plugin))
                return Task.FromException(Errors.PluginNotFound(id));

            return plugin.Start();
        }

        private Task StopPlugin(StopPluginRequest request)
        {
            var id = request.PluginId;
            if (!_plugins.TryGetValue(id, out var plugin))
                return Task.FromException(Errors.PluginNotFound(id));

            return plugin.Stop();
        }


        private PluginModel CreatePluginIntenal(string id, PluginSavedState savedState)
        {
            var plugin = new PluginModel(PluginActor.Create(_server, savedState));
            _plugins[id] = plugin;
            return plugin;
        }


        internal class ShutdownCmd { }

        internal class RestoreCmd { }
    }
}
