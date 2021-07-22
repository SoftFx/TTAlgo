using Google.Protobuf;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.CoreV1;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Runtime
{
    internal class ExecutorV1Actor : Actor
    {
        public const int AbortTimeout = 10000;

        private readonly string _id;
        private readonly ExecutorConfig _config;
        private readonly IActorRef _runtime, _account;

        private IAlgoLogger _logger;
        private RuntimeInfoProvider _provider;
        private PluginExecutorCore _executor;
        private Executor.Types.State _state;


        private ExecutorV1Actor(ExecutorConfig config, IActorRef runtime, IActorRef account)
        {
            _id = config.Id;
            _config = config;
            _runtime = runtime;
            _account = account;

            Receive<StartExecutorRequest>(Start);
            Receive<StopExecutorRequest>(Stop);
            Receive<StoppedMsg>(OnStopped);
            Receive<ExitedMsg>(OnExited);
            Receive<ConnectionStateUpdate>(OnConnectionStateUpdated);
        }


        public static IActorRef Create(ExecutorConfig config, IActorRef runtime, IActorRef account)
        {
            return ActorSystem.SpawnLocal(() => new ExecutorV1Actor(config, runtime, account), $"{nameof(ExecutorV1Actor)} ({config.Id})");
        }


        protected override void ActorInit(object initMsg)
        {
            _logger = AlgoLoggerFactory.GetLogger(Name);

            _state = Executor.Types.State.Stopped;
        }


        private async Task Start(StartExecutorRequest request)
        {
            if (!_state.IsStopped())
                throw new AlgoException("Executor already started");

            ChangeState(Executor.Types.State.WaitConnect);

            var accId = _config.AccountId;
            try
            {
                _provider = await _account.Ask<RuntimeInfoProvider>(AttachedAccountActor.GetInfoProviderRequest.Instance);
                await _account.Ask(new AttachedAccountActor.AddRefCmd(_id, Self));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to attach account {accId}");
                ChangeState(Executor.Types.State.Faulted);
            }
        }

        private async Task Stop(StopExecutorRequest request)
        {
            if (_state.IsStopped())
                throw new AlgoException("Executor already stopped");

            if (_state.IsWaitConnect())
            {
                OnStopped(StoppedMsg.Instance);
                return;
            }

            if (_state.IsRunning() || _state.IsWaitReconnect())
            {
                ChangeState(Executor.Types.State.Stopping);

                var stopTask = _executor.Stop();

                var delayTask = Task.Delay(AbortTimeout);

                var t = await Task.WhenAny(stopTask, delayTask);

                if (t == delayTask)
                {
                    _logger.Info("Executor didn't stop within timeout. Aborting...");
                    _executor.Abort();
                }
                else if (stopTask.IsFaulted)
                {
                    _logger.Error(stopTask.Exception, "Failed to stop executor");
                    DetachAccount();
                }
            }
        }

        private void OnStopped(StoppedMsg msg)
        {
            ChangeState(Executor.Types.State.Stopped);

            DetachAccount();

            _provider.Dispose();
        }

        private void OnExited(ExitedMsg msg)
        {
            _logger.Debug("Received exit request");

            _runtime.Tell(new ExecutorNotification(_id, new PluginExitedMsg { Id = _id }));

            Self.Tell(new StopExecutorRequest());
        }

        private void OnConnectionStateUpdated(ConnectionStateUpdate update)
        {
            if (update.NewState.IsOnline() && _state.IsWaitConnect())
            {
                var _ = StartExecutorInternal();
            }
            else if (update.NewState.IsOnline() && _state.IsWaitReconnect())
            {
                HandleReconnectInternal()
                    .OnException(ex => _logger.Error(ex, "Failed to handle reconnect"));
            }
            else if (!update.NewState.IsOnline() && _state.IsRunning())
            {
                ChangeState(Executor.Types.State.WaitReconnect);
                _executor.HandleDisconnect();
            }
        }


        private void ChangeState(Executor.Types.State newState)
        {
            var update = new ExecutorStateUpdate(_id, _state, newState);
            _state = newState;
            _runtime.Tell(update);

            _logger.Info($"State changed: {update.OldState} -> {update.NewState}");
        }

        private void DetachAccount()
        {
            _account.Ask(new AttachedAccountActor.RemoveRefCmd(_id))
                .OnException(ex => _logger.Error(ex, $"Failed to detach account {_config.AccountId}"));
        }

        private async Task StartExecutorInternal()
        {
            try
            {
                _logger.Debug("Starting internal executor...");

                var config = _config.PluginConfig.Unpack<PluginConfig>();

                _executor = new PluginExecutorCore(config.Key);
                _executor.Stopped += _ => Self.Tell(StoppedMsg.Instance);
                _executor.OnExitRequest += _ => Self.Tell(ExitedMsg.Instance);
                _executor.OnNotification += msg => _runtime.Tell(new ExecutorNotification(_id, msg));
                _executor.OnInternalError += ex => _logger.Error(ex, "Internal error in executor");
                _executor.IsGlobalMarshalingEnabled = true;
                _executor.IsBunchingRequired = true;

                await _provider.PreLoad();
                var accProxy = new LocalAccountProxy(_config.AccountId)
                {
                    Metadata = _provider,
                    Feed = _provider,
                    FeedHistory = _provider,
                    AccInfoProvider = _provider,
                    TradeExecutor = _provider,
                    TradeHistoryProvider = _provider,
                };

                _executor.ApplyConfig(_config, config, accProxy);

                _executor.Start();

                ChangeState(Executor.Types.State.Running);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to start executor");
                ChangeState(Executor.Types.State.Faulted);
            }
        }

        private async Task HandleReconnectInternal()
        {
            await _provider.PreLoad();
            _executor.HandleReconnect();
            ChangeState(Executor.Types.State.Running);
        }


        internal class StoppedMsg : Singleton<StoppedMsg> { }

        internal class ExitedMsg : Singleton<ExitedMsg> { }

        internal class ExecutorNotification
        {
            public string ExecutorId { get; }

            public IMessage Message { get; }

            public ExecutorNotification(string executorId, IMessage message)
            {
                ExecutorId = executorId;
                Message = message;
            }
        }
    }
}
