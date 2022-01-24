using System;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Backtester;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Ext;
using TickTrader.Algo.Package;

namespace TickTrader.Algo.BacktesterV1Host
{
    internal class BacktesterV1Actor : Actor
    {
        private readonly string _id;
        private BacktesterV1HostHandler _handler;

        private IAlgoLogger _logger;
        private CancellationTokenSource _cancelTokenSrc;


        private BacktesterV1Actor(string id, BacktesterV1HostHandler handler)
        {
            _id = id;
            _handler = handler;

            Receive<StartBacktesterRequest>(Start);
            Receive<StopBacktesterRequest>(Stop);
        }


        public static IActorRef Create(string id, BacktesterV1HostHandler handler)
        {
            return ActorSystem.SpawnLocal(() => new BacktesterV1Actor(id, handler), $"{nameof(BacktesterV1Actor)} ({id})");
        }


        protected override void ActorInit(object initMsg)
        {
            _logger = AlgoLoggerFactory.GetLogger(Name);
        }


        private void Start(StartBacktesterRequest request)
        {
            _logger.Debug("Starting...");

            // load default reduction to metadata cache
            PackageExplorer.ScanAssembly(MappingDefaults.DefaultExtPackageId, typeof(BarCloseReduction).Assembly);

            var config = BacktesterConfig.Load(request.ConfigPath);
            _logger.Debug($"Mode = {config.Core.Mode}");
            _logger.Debug($"PkgId = {config.PluginConfig.Key.PackageId}");
            _logger.Debug($"PluginId = {config.PluginConfig.Key.DescriptorId}");

            _logger.Debug("Started successfully");

            _cancelTokenSrc = new CancellationTokenSource();
            var _ = RunInternal(config);
        }

        private async Task Stop(StopBacktesterRequest request)
        {
            _logger.Debug("Stopping...");

            _cancelTokenSrc?.Cancel();

            _logger.Debug("Stopped");
        }


        private async Task RunInternal(BacktesterConfig config)
        {
            try
            {
                const ulong total = 10;
                for (ulong i = 0; i < total; i++)
                {
                    await Task.Delay(1000, _cancelTokenSrc.Token);
                    _handler.SendProgress(i, total);
                }

                _handler.SendStoppedMsg(null);

                _logger.Debug("Finished");
            }
            catch (Exception ex)
            {
                _handler.SendStoppedMsg(ex.Message);
            }
        }
    }
}
