using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.Algo.Server
{
    public class PluginManager
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<RuntimeManager>();

        private readonly IActorRef _impl;


        public PluginManager(AlgoServer server)
        {
            _impl = ActorSystem.SpawnLocal<Impl>(nameof(PluginManager), new InitMsg(server));
        }


        private class InitMsg
        {
            public AlgoServer Server { get; }

            public InitMsg(AlgoServer server)
            {
                Server = server;
            }
        }


        private class Impl : Actor
        {
            private readonly Dictionary<string, PluginModel> _plugins = new Dictionary<string, PluginModel>();

            private AlgoServer _server;


            public Impl()
            {
                Receive<AddPluginRequest>(AddPlugin);
                Receive<RemovePluginRequest>(RemovePlugin);
                Receive<StartPluginRequest>(StartPlugin);
                Receive<StopPluginRequest>(StopPlugin);
            }


            protected override void ActorInit(object initMsg)
            {
                var msg = (InitMsg)initMsg;
                _server = msg.Server;
            }


            private Task<PluginModel> AddPlugin(AddPluginRequest request)
            {
                var id = request.Config.InstanceId;
                if (_plugins.ContainsKey(id))
                    return Task.FromException<PluginModel>(Errors.DuplicatePlugin(id));

                var plugin = new PluginModel(_server, request);
                _plugins[id] = plugin;
                return Task.FromResult(plugin);
            }

            private void RemovePlugin(RemovePluginRequest request)
            {
            }

            private Task StartPlugin(StartPluginRequest request)
            {
                var id = request.PluginId;

                if (!_plugins.TryGetValue(id, out var plugin))
                    return Task.FromException(Errors.PluginNotFound(id));

                return plugin.Start(request);
            }

            private Task StopPlugin(StopPluginRequest request)
            {
                var id = request.PluginId;

                if (!_plugins.TryGetValue(id, out var plugin))
                    return Task.FromException(Errors.PluginNotFound(id));

                return plugin.Stop(request);
            }
        }
    }
}
