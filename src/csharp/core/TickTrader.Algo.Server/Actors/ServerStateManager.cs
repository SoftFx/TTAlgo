using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Server.Persistence;

namespace TickTrader.Algo.Server
{
    internal class ServerStateManager : Actor
    {
        public const int SaveDelay = 1000;

        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<ServerStateManager>();

        private readonly string _stateFilePath;

        private CancellationTokenSource _cancelTokenSrc;
        private ServerSavedState _state;
        private int _stateCnt, _lastSavedStateCnt;
        private bool _isStopped;


        private ServerStateManager(string stateFilePath)
        {
            _stateFilePath = stateFilePath;

            Receive<StopSavingCmd>(StopSaving);
            Receive<SaveCmd>(Save);
            Receive<StateSnapshotRequest, ServerSavedState>(GetSnapshot);
            Receive<AddAccountCmd, object>(AddAccount);
            Receive<RemoveAccountCmd, object>(RemoveAccount);
            Receive<UpdateAccountCmd, object>(UpdateAccount);
            Receive<AddPluginCmd, object>(AddPlugin);
            Receive<UpdatePluginCmd, object>(UpdatePlugin);
            Receive<RemovePluginCmd, object>(RemovePlugin);
        }


        public static IActorRef Create(string serverStatePath)
        {
            return ActorSystem.SpawnLocal(() => new ServerStateManager(serverStatePath), nameof(ServerStateManager));
        }


        protected override void ActorInit(object initMsg)
        {
            if (!Load())
            {
                _state = new ServerSavedState();
            }
            _cancelTokenSrc = new CancellationTokenSource();
            _stateCnt = _lastSavedStateCnt = 0;
            _isStopped = false;
            ScheduleSave(SaveDelay);
        }


        private bool Load()
        {
            if (File.Exists(_stateFilePath))
            {
                try
                {
                    _state = JsonSerializer.Deserialize<ServerSavedState>(File.ReadAllText(_stateFilePath));
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to load server state");
                }
            }
            return false;
        }

        private void ScheduleSave(int saveDelay)
        {
            Task.Delay(saveDelay, _cancelTokenSrc.Token)
                .ContinueWith(t =>
                {
                    if (t.IsCompleted)
                    {
                        Self.Tell(new SaveCmd());
                    }
                });
        }

        private bool Save()
        {
            if (_lastSavedStateCnt != _stateCnt)
            {
                try
                {
                    File.WriteAllText(_stateFilePath, JsonSerializer.Serialize(_state));
                    _lastSavedStateCnt = _stateCnt;

                    _logger.Debug($"Saved server state {_stateCnt}");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to save server state");
                    return true;
                }
            }
            return false;
        }

        private void IncreaseStateCnt()
        {
            _stateCnt++;
            _logger.Debug($"Server state changed to {_stateCnt}");
        }


        private void StopSaving(StopSavingCmd cmd)
        {
            _logger.Debug("Stopping...");

            _isStopped = true;
            _cancelTokenSrc.Cancel();
            Save();

            _logger.Debug("Stopped");
        }

        private void Save(SaveCmd cmd)
        {
            if (_isStopped)
                return;

            var hasError = Save();
            ScheduleSave(hasError ? 10 * SaveDelay : SaveDelay);
        }

        private ServerSavedState GetSnapshot(StateSnapshotRequest request)
        {
            return _state.Clone();
        }

        private object AddAccount(AddAccountCmd cmd)
        {
            if (_isStopped)
                return null;

            var accId = cmd.Account.Id;
            if (_state.Accounts.ContainsKey(accId))
                return Errors.DuplicateAccount(accId);

            _state.Accounts.Add(accId, cmd.Account);
            IncreaseStateCnt();

            return null;
        }

        private object UpdateAccount(UpdateAccountCmd cmd)
        {
            if (_isStopped)
                return null;

            var accId = cmd.Account.Id;
            if (!_state.Accounts.Remove(accId))
                return Errors.AccountNotFound(accId);

            _state.Accounts[accId] = cmd.Account;
            IncreaseStateCnt();

            return null;
        }

        private object RemoveAccount(RemoveAccountCmd cmd)
        {
            if (_isStopped)
                return null;

            var accId = cmd.Id;
            if (!_state.Accounts.Remove(accId))
                return Errors.AccountNotFound(accId);

            IncreaseStateCnt();

            return null;
        }

        private object AddPlugin(AddPluginCmd cmd)
        {
            if (_isStopped)
                return null;

            var id = cmd.Plugin.Id;
            if (_state.Plugins.ContainsKey(id))
                return Errors.DuplicatePlugin(id);

            _state.Plugins.Add(id, cmd.Plugin);
            IncreaseStateCnt();

            return null;
        }

        private object UpdatePlugin(UpdatePluginCmd cmd)
        {
            if (_isStopped)
                return null;

            var id = cmd.Plugin.Id;
            if (!_state.Plugins.ContainsKey(id))
                return Errors.PluginNotFound(id);

            _state.Plugins[id] = cmd.Plugin;
            IncreaseStateCnt();

            return null;
        }

        private object RemovePlugin(RemovePluginCmd cmd)
        {
            if (_isStopped)
                return null;

            var id = cmd.Id;
            if (!_state.Plugins.Remove(id))
                return Errors.PluginNotFound(id);

            IncreaseStateCnt();

            return null;
        }

        private object SetPluginRunning(SetPluginRunningCmd cmd)
        {
            if (_isStopped)
                return null;

            var id = cmd.Id;
            if (!_state.Plugins.TryGetValue(id, out var plugin))
                return Errors.PluginNotFound(id);

            plugin.IsRunning = cmd.IsRunning;
            IncreaseStateCnt();

            return null;
        }


        internal class StopSavingCmd { }

        internal class SaveCmd { }

        internal class StateSnapshotRequest { }

        internal class AddAccountCmd
        {
            public AccountSavedState Account { get; }

            public AddAccountCmd(AccountSavedState account)
            {
                Account = account;
            }
        }

        internal class UpdateAccountCmd
        {
            public AccountSavedState Account { get; }

            public UpdateAccountCmd(AccountSavedState account)
            {
                Account = account;
            }
        }

        internal class RemoveAccountCmd
        {
            public string Id { get; }

            public RemoveAccountCmd(string id)
            {
                Id = id;
            }
        }

        internal class AddPluginCmd
        {
            public PluginSavedState Plugin { get; }

            public AddPluginCmd(PluginSavedState plugin)
            {
                Plugin = plugin;
            }
        }

        internal class UpdatePluginCmd
        {
            public PluginSavedState Plugin { get; }

            public UpdatePluginCmd(PluginSavedState plugin)
            {
                Plugin = plugin;
            }
        }

        internal class RemovePluginCmd
        {
            public string Id { get; }

            public RemovePluginCmd(string id)
            {
                Id = id;
            }
        }

        internal class SetPluginRunningCmd
        {
            public string Id { get; }

            public bool IsRunning { get; }

            public SetPluginRunningCmd(string id, bool isRunning)
            {
                Id = id;
                IsRunning = isRunning;
            }
        }
    }
}
