using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Runtime
{
    internal class AttachedAccountActor : Actor
    {
        private readonly string _id;
        private readonly PluginRuntimeV1Handler _handler;
        private readonly Dictionary<string, IActorRef> _executors = new Dictionary<string, IActorRef>();

        private IAlgoLogger _logger;
        private RemoteAccountProxy _accProxy;
        private Account.Types.ConnectionState _state;
        private int _stateCnt, _refCnt;
        private bool _attached, _attachLocked;


        private AttachedAccountActor(string id, PluginRuntimeV1Handler handler)
        {
            _id = id;
            _handler = handler;

            Receive<ConnectionStateUpdate>(OnConnectionStateUpdated);
            Receive<AddRefCmd>(AddRef);
            Receive<RemoveRefCmd>(RemoveRef);
            Receive<ManageAttachCmd>(ManageAttachLoop);
            Receive<GetInfoProviderRequest, RuntimeInfoProvider>(_ => new RuntimeInfoProvider(_accProxy));
        }


        public static IActorRef Create(string id, PluginRuntimeV1Handler handler)
        {
            return ActorSystem.SpawnLocal(() => new AttachedAccountActor(id, handler), $"{nameof(AttachedAccountActor)} ({id})");
        }


        protected override void ActorInit(object initMsg)
        {
            _logger = AlgoLoggerFactory.GetLogger(Name);
            _state = Account.Types.ConnectionState.Offline;
            _stateCnt = 0;
            _accProxy = new RemoteAccountProxy(_id, Self);
        }


        private void OnConnectionStateUpdated(ConnectionStateUpdate update)
        {
            _logger.Info($"Received state change: {update.OldState} -> {update.NewState}");

            if (update.NewState.IsOffline() || update.NewState.IsDisconnecting())
            {
                var _ = DeinitAccount();
            }
            else if (update.NewState.IsOnline())
            {
                var _ = InitAccount();
            }
        }

        private void AddRef(AddRefCmd cmd)
        {
            var executorId = cmd.ExecutorId;
            var executor = cmd.Executor;
            _executors.Add(executorId, executor);
            executor.Tell(new ConnectionStateUpdate(_id, _state, _state));

            _refCnt++;
            _logger.Debug($"Added ref from '{executorId}'. Active refs cnt = {_refCnt}");

            Self.Tell(ManageAttachCmd.Instance);
        }

        private void RemoveRef(RemoveRefCmd cmd)
        {
            var executorId = cmd.ExecutorId;
            _executors.Remove(executorId);

            _refCnt--;
            _logger.Debug($"Removed ref from '{executorId}'. Active refs cnt = {_refCnt}");

            Self.Tell(ManageAttachCmd.Instance);
        }

        private async Task ManageAttachLoop(ManageAttachCmd cmd)
        {
            if (_attachLocked)
                return;

            _attachLocked = true;
            try
            {
                if (!_attached && _refCnt > 0)
                {
                    _logger.Debug($"Attaching account {_id}. Active refs cnt = {_refCnt}");
                    try
                    {
                        await _handler.AttachAccount(_id, _accProxy);
                        _attached = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, $"Failed to attach account {_id}");
                        _attached = false;
                    }
                    Self.Tell(ManageAttachCmd.Instance);
                }
                else if (_attached && _refCnt < 1)
                {
                    _logger.Debug($"Detaching account {_id}. Active refs cnt = {_refCnt}");
                    try
                    {
                        await DeinitAccount();
                        await _handler.DetachAccount(_id);
                        _attached = false;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, $"Failed to detach account {_id}");
                        _attached = false;
                    }
                }
            }
            finally
            {
                _attachLocked = false;
            }
        }


        private void ChangeState(Account.Types.ConnectionState newState)
        {
            if (_state == newState)
                return;

            var update = new ConnectionStateUpdate(_id, _state, newState);
            _state = newState;
            _stateCnt++;

            _logger.Info($"State changed: {update.OldState} -> {update.NewState}");
            foreach(var executor in _executors.Values)
            {
                executor.Tell(update);
            }
        }

        private async Task InitAccount()
        {
            var currentState = _stateCnt;

            try
            {
                ChangeState(Account.Types.ConnectionState.Connecting);

                await _accProxy.Start();

                if (_stateCnt == currentState)
                {
                    ChangeState(Account.Types.ConnectionState.Online);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to init attached account");
                ChangeState(Account.Types.ConnectionState.Offline);
            }
        }

        private async Task DeinitAccount()
        {
            try
            {
                ChangeState(Account.Types.ConnectionState.Offline);
                await _accProxy.Stop();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to deinit attached account");
            }
        }


        internal class AddRefCmd
        {
            public string ExecutorId { get; }

            public IActorRef Executor { get; }

            public AddRefCmd(string executorId, IActorRef executor)
            {
                ExecutorId = executorId;
                Executor = executor;
            }
        }

        internal class RemoveRefCmd
        {
            public string ExecutorId { get; }

            public RemoveRefCmd(string executorId)
            {
                ExecutorId = executorId;
            }
        }

        internal class ManageAttachCmd : Singleton<ManageAttachCmd> { }

        internal class GetInfoProviderRequest : Singleton<GetInfoProviderRequest> { }
    }
}
