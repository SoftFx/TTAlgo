using Google.Protobuf.WellKnownTypes;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.Algo.Server
{
    public class PluginModel
    {
        private readonly IActorRef _impl;


        public PluginModel(AlgoServer server, AddPluginRequest request)
        {
            _impl = ActorSystem.SpawnLocal<Impl>($"{nameof(PluginModel)} ({request.Config.InstanceId})", new InitMsg(server, request));
        }


        public Task Start(StartPluginRequest request) => _impl.Ask(request);

        public Task Stop(StopPluginRequest request) => _impl.Ask(request);


        private class InitMsg
        {
            public AlgoServer Server { get; }

            public AddPluginRequest AddRequest { get; }

            public InitMsg(AlgoServer server, AddPluginRequest addRequest)
            {
                Server = server;
                AddRequest = addRequest;
            }
        }


        private class Impl : Actor
        {
            private IAlgoLogger _logger;
            private AlgoServer _server;
            private PluginConfig _config;
            private string _accountId;
            private string _id;

            private PluginModelInfo.Types.PluginState _state;
            private PkgRuntimeModel _runtime;
            private PluginInfo _pluginInfo;
            private string _faultMsg;


            public Impl()
            {
                Receive<StartPluginRequest>(Start);
                Receive<StopPluginRequest>(Stop);
            }


            protected override void ActorInit(object initMsg)
            {
                _logger = AlgoLoggerFactory.GetLogger(Name);

                var msg = (InitMsg)initMsg;
                _server = msg.Server;
                _config = msg.AddRequest.Config;
                _accountId = msg.AddRequest.AccountId;

                _id = _config.InstanceId;
            }


            private async Task Start(StartPluginRequest request)
            {
                await UpdateRuntime();

                var config = new ExecutorConfig { AccountId = _accountId, PluginConfig = Any.Pack(_config) };
                config.WorkingDirectory = _server.Env.GetPluginWorkingFolder(_id);
                config.InitPriorityInvokeStrategy();
                config.InitSlidingBuffering(4000);
                config.InitBarStrategy(Feed.Types.MarketSide.Bid);

                await _runtime.CreateExecutor(_id, config);
            }

            private Task Stop(StopPluginRequest request)
            {
                return Task.CompletedTask;
            }


            private async Task UpdateRuntime()
            {
                var pluginKey = _config.Key;
                var pkgId = pluginKey.PackageId;

                _runtime = await _server.Runtimes.GetPkgRuntime(pkgId);
                if (_runtime == null)
                {
                    BreakBot($"Algo package {pkgId} is not found");
                    return;
                }

                _pluginInfo = await _runtime.GetPluginInfo(pluginKey);
                if (_pluginInfo == null)
                {
                    BreakBot($"Trade bot '{pluginKey.DescriptorId}' is missing in Algo package '{pkgId}'");
                    return;
                }

                if (_state == PluginModelInfo.Types.PluginState.Broken)
                    ChangeState(PluginModelInfo.Types.PluginState.Stopped, null);
            }

            private void BreakBot(string message)
            {
                ChangeState(PluginModelInfo.Types.PluginState.Broken, message);
            }

            private void ChangeState(PluginModelInfo.Types.PluginState newState, string faultMsg = null)
            {
                if (string.IsNullOrWhiteSpace(faultMsg))
                    _logger.Info($"State: {newState}", newState);
                else
                    _logger.Error($"State: {newState} Error: {faultMsg}");
                _state = newState;
                _faultMsg = faultMsg;
                //StateChanged?.Invoke(new PluginStateUpdate { PluginId = _config.InstanceId, State = newState, FaultMessage = faultMsg });
            }
        }
    }
}
