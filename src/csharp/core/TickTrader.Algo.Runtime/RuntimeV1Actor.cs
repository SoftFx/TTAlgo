using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Ext;
using TickTrader.Algo.Indicators.Trend.MovingAverage;
using TickTrader.Algo.Package;

namespace TickTrader.Algo.Runtime
{
    internal class RuntimeV1Actor : Actor
    {
        private readonly string _id;
        private readonly PluginRuntimeV1Handler _handler;
        private readonly Dictionary<string, IActorRef> _executorsMap = new Dictionary<string, IActorRef>();
        private readonly Dictionary<string, IActorRef> _accountsMap = new Dictionary<string, IActorRef>();

        private IAlgoLogger _logger;
        private RuntimeConfig _config;
        private PackageInfo _pkgInfo;


        private RuntimeV1Actor(string id, PluginRuntimeV1Handler handler)
        {
            _id = id;
            _handler = handler;

            Receive<StartRuntimeRequest>(Start);
            Receive<StopRuntimeRequest>(Stop);
            Receive<StartExecutorRequest>(StartExecutor);
            Receive<StopExecutorRequest>(StopExecutor);
            Receive<ExecutorStateUpdate>(OnExecutorStateUpdated);
            Receive<ExecutorV1Actor.ExecutorNotification>(OnExecutorNotification);
        }


        public static IActorRef Create(string id, PluginRuntimeV1Handler handler)
        {
            return ActorSystem.SpawnLocal(() => new RuntimeV1Actor(id, handler), $"{nameof(RuntimeV1Actor)} ({id})");
        }


        protected override void ActorInit(object initMsg)
        {
            _logger = AlgoLoggerFactory.GetLogger(Name);
        }


        private async Task Start(StartRuntimeRequest request)
        {
            _logger.Debug("Starting...");

            // load default reduction to metadata cache
            PackageExplorer.ScanAssembly(MappingDefaults.DefaultExtPackageId, typeof(BarCloseReduction).Assembly);

            _config = await _handler.GetRuntimeConfig().ConfigureAwait(false);

            var pkgId = _config.PackageId;
            var pkgPath = _config.PackageIdentity.FilePath;

            _logger.Debug($"Loading package '{pkgId}'");

            _logger.Debug($"Scanning package '{pkgId}' at {pkgPath}");
            if (pkgPath.EndsWith("TickTrader.Algo.Indicators.dll", StringComparison.OrdinalIgnoreCase))
            {
                _pkgInfo = PackageExplorer.ScanAssembly(pkgId, typeof(MovingAverage).Assembly);
            }
            else
            {
                _pkgInfo = PackageLoadContext.Load(_config.PackageId, _config.PackageBinary.ToByteArray());
            }

            _pkgInfo.Identity = _config.PackageIdentity;

            _logger.Debug("Started successfully");
        }

        private async Task Stop(StopRuntimeRequest request)
        {
            _logger.Debug("Stopping...");

            var stopTasks = _executorsMap.Select(e => StopExecutor(e.Key, e.Value));
            await Task.WhenAll(stopTasks);

            _logger.Debug("Stopped");
        }

        private async Task StartExecutor(StartExecutorRequest request)
        {
            var executorId = request.ExecutorId;
            if (_executorsMap.ContainsKey(executorId))
                throw new AlgoException("Executor already started");

            var config = await _handler.GetExecutorConfig(executorId);

            var acc = GetOrCreateAccount(config.AccountId);

            var executor = ExecutorV1Actor.Create(config, Self, acc);
            _executorsMap.Add(executorId, executor);

            try
            {
                await executor.Ask(request);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to start executor {executorId}");
                _executorsMap.Remove(executorId);

                throw;
            }
        }

        private async Task StopExecutor(StopExecutorRequest request)
        {
            var executorId = request.ExecutorId;
            if (!_executorsMap.TryGetValue(executorId, out var executor))
                throw new AlgoException("Executor not found");

            try
            {
                await StopExecutor(executorId, executor);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to stop executor {executorId}");
            }
        }

        private void OnExecutorStateUpdated(ExecutorStateUpdate update)
        {
            var executorId = update.ExecutorId;
            var newState = update.NewState;

            _handler.SendNotification(executorId, update);
            if (newState.IsFaulted() || newState.IsStopped())
            {
                if (_executorsMap.TryGetValue(executorId, out var executor))
                {
                    ActorSystem.StopActor(executor)
                        .OnException(ex => _logger.Error(ex, $"Failed to stop actor {executor.Name}"));
                    _executorsMap.Remove(executorId);
                }
            }
        }

        private void OnExecutorNotification(ExecutorV1Actor.ExecutorNotification notification)
        {
            _handler.SendNotification(notification.ExecutorId, notification.Message);
        }


        private async Task<bool> StopExecutor(string id, IActorRef executor)
        {
            try
            {
                await executor.Ask(new StopExecutorRequest());
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to stop executor {id}");
            }

            return false;
        }

        private IActorRef GetOrCreateAccount(string accId)
        {
            if (!_accountsMap.TryGetValue(accId, out var account))
            {
                account = AttachedAccountActor.Create(accId, _handler);
                _accountsMap.Add(accId, account);
            }

            return account;
        }
    }
}
