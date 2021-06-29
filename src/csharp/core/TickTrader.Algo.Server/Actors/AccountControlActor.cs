﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Account;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Server.Persistence;

namespace TickTrader.Algo.Server
{
    internal class AccountControlActor : Actor
    {
        private static readonly TimeSpan KeepAliveThreshold = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan ReconnectThreshold = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan ReconnectThreshold_AuthProblem = TimeSpan.FromMinutes(1);

        private readonly AlgoServer _server;
        private readonly string _id;

        private AccountSavedState _savedState;
        private AccountCreds _creds;
        private IAlgoLogger _logger;
        private ActorSharp.Lib.AsyncGate _requestGate;
        private AccountModelInfo.Types.ConnectionState _state;
        private ConnectionErrorInfo _lastError;
        private int _startedBotsCount;
        private bool _isInitialized, _credsChanged, _lostConnection;
        private TaskCompletionSource<object> _initCompletionSrc, _shutdownCompletionSrc;
        private DateTime _pendingDisconnect, _pendingReconnect;
        private ClientModel.ControlHandler2 _core;
        private IAccountProxy _accProxy;


        private AccountControlActor(AlgoServer server, AccountSavedState savedState)
        {
            _server = server;
            _savedState = savedState;
            _id = savedState.Id;
            _creds = savedState.UnpackCreds();

            Receive<ShutdownCmd>(Shutdown);
            Receive<ChangeAccountRequest>(Change);
            Receive<AccountMetadataRequest, AccountMetadataInfo>(GetMetadata);

            Receive<ManageConnectionCmd>(ManageConnectionLoop);
            Receive<ScheduleDisconnectCmd>(ScheduleDisconnect);
            Receive<ConnectionLostMsg>(OnConnectionLost);
        }


        public static IActorRef Create(AlgoServer server, AccountSavedState savedState)
        {
            return ActorSystem.SpawnLocal(() => new AccountControlActor(server, savedState), $"{nameof(AccountControlActor)} ({savedState.Id})");
        }


        protected override void ActorInit(object initMsg)
        {
            _logger = AlgoLoggerFactory.GetLogger(Name);

            _state = AccountModelInfo.Types.ConnectionState.Offline;
            _requestGate = new ActorSharp.Lib.AsyncGate();
            _requestGate.OnWait += () => Self.Tell(ManageConnectionCmd.Instance);
            _requestGate.OnExit += () => Self.Tell(ScheduleDisconnectCmd.Instance);

            _initCompletionSrc = new TaskCompletionSource<object>();
            InitInternal().Forget();
        }


        private Task Shutdown(ShutdownCmd cmd)
        {
            if (_shutdownCompletionSrc != null)
                return _shutdownCompletionSrc.Task;

            _shutdownCompletionSrc = new TaskCompletionSource<object>();
            ShutdownInternal().Forget();
            return _shutdownCompletionSrc.Task;
        }

        private async Task Change(ChangeAccountRequest request)
        {
            var changed = false;

            if (!string.IsNullOrEmpty(request.DisplayName))
            {
                _savedState.DisplayName = request.DisplayName;
                changed = true;
            }
            if (request.Creds != null && _creds.Merge(request.Creds))
            {
                _savedState.PackCreds(_creds);
                changed = true;
                _credsChanged = true;
            }

            if (changed)
            {
                await _server.SavedState.UpdateAccount(_savedState);
                if (_credsChanged)
                    ManageConnectionInternal();
                //Changed?.Invoke(this);
            }
        }

        public async Task<AccountMetadataInfo> GetMetadata(AccountMetadataRequest request)
        {
            using (await _requestGate.Enter())
            {
                if (!_lastError.IsOk)
                    throw new AlgoException($"Connection error! Code: {_lastError.Code}");

                var symbols = await _core.GetSymbols();
                var defaultSymbol = await _core.GetDefaultSymbol();
                return new AccountMetadataInfo(_id, symbols.Select(s => s.ToInfo()).ToList(), defaultSymbol.ToInfo());
            }
        }

        private void ManageConnectionLoop(ManageConnectionCmd cmd)
        {
            if (_shutdownCompletionSrc != null)
                return;

            ManageConnectionInternal();
            TaskExt.Schedule(1000, () => Self.Tell(ManageConnectionCmd.Instance));
        }

        private void OnConnectionLost(ConnectionLostMsg msg)
        {
            _lostConnection = true;
            ManageConnectionInternal();
        }

        private void ScheduleDisconnect(ScheduleDisconnectCmd cmd)
        {
            _pendingDisconnect = DateTime.UtcNow + KeepAliveThreshold;
        }

        private void ScheduleReconnect(bool authProblem)
        {
            _pendingReconnect = DateTime.UtcNow + (authProblem ? ReconnectThreshold_AuthProblem : ReconnectThreshold);
        }

        private void ManageConnectionInternal()
        {
            if (_state == AccountModelInfo.Types.ConnectionState.Offline)
            {
                var forcedConnect = (_startedBotsCount > 0 && _credsChanged) || _requestGate.WatingCount > 0;
                var scheduledConnect = _startedBotsCount > 0 && _pendingReconnect < DateTime.UtcNow;

                if (forcedConnect || scheduledConnect)
                    ConnectInternal().OnException(ex => _logger.Error(ex, "Connect failed"));
            }
            else if (_state == AccountModelInfo.Types.ConnectionState.Online)
            {
                var forcedDisconnect = _credsChanged || _lostConnection || _shutdownCompletionSrc != null;
                var scheduledDisconnect = _startedBotsCount == 0 && _pendingDisconnect < DateTime.UtcNow;

                if (forcedDisconnect || scheduledDisconnect)
                    DisconnectInternal().OnException(ex => _logger.Error(ex, "Disconnect failed"));
            }
        }

        private async Task InitInternal()
        {
            try
            {
                var options = new ConnectionOptions { EnableLogs = false, LogsFolder = _server.Env.LogFolder, Type = AppType.BotAgent };
                _core = new ClientModel.ControlHandler2(KnownAccountFactories.Fdk2, options,
                        _server.Env.FeedHistoryCacheFolder, FeedHistoryFolderOptions.ServerClientHierarchy, _id);

                await _core.OpenHandler();

                _core.Connection.Disconnected += () => Self.Tell(ConnectionLostMsg.Instance);

                var feedAdapter = await _core.CreateFeedProvider();
                var tradeApi = await _core.CreateTradeApi();
                var tradeInfo = await _core.CreateTradeProvider();
                var tradeHistory = await _core.CreateTradeHistory();

                _accProxy = new LocalAccountProxy(_id)
                {
                    Feed = feedAdapter,
                    FeedHistory = feedAdapter,
                    Metadata = feedAdapter,
                    AccInfoProvider = tradeInfo,
                    TradeExecutor = tradeApi,
                    TradeHistoryProvider = tradeHistory.AlgoAdapter,
                };

                Self.Tell(ManageConnectionCmd.Instance);

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to init account '{_id}'");
            }

            _initCompletionSrc.TrySetResult(null);
        }

        private async Task ShutdownInternal()
        {
            try
            {
                _logger.Debug("Stopping...");

                await _initCompletionSrc.Task;

                if (_state == AccountModelInfo.Types.ConnectionState.Offline)
                    _shutdownCompletionSrc.TrySetResult(null);
                else
                    ManageConnectionInternal();

                await _core.CloseHandler();

                _logger.Debug("Stopped");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to shutdown account '{_id}'");
                _shutdownCompletionSrc.TrySetResult(null);
            }
        }

        private async Task ConnectInternal()
        {
            if (!_isInitialized)
                return;

            ChangeState(AccountModelInfo.Types.ConnectionState.Connecting);
            _credsChanged = false;

            var savedState = _savedState;
            _lastError = await _core.Connection.Connect(savedState.UserId, _creds.GetPassword(), savedState.Server, CancellationToken.None);

            if (_lastError.Code == ConnectionErrorInfo.Types.ErrorCode.NoConnectionError)
            {
                _lostConnection = false;
                ScheduleDisconnect(ScheduleDisconnectCmd.Instance);
                ChangeState(AccountModelInfo.Types.ConnectionState.Online);

                _requestGate.Open();
            }
            else
            {
                await _requestGate.ExecQueuedRequests();

                ScheduleReconnect(_lastError.Code == ConnectionErrorInfo.Types.ErrorCode.BlockedAccount || _lastError.Code == ConnectionErrorInfo.Types.ErrorCode.InvalidCredentials);
                ChangeState(AccountModelInfo.Types.ConnectionState.Offline);
            }
        }

        private async Task DisconnectInternal()
        {
            if (!_isInitialized)
                return;

            ChangeState(AccountModelInfo.Types.ConnectionState.Disconnecting);

            _logger.Debug("Closing gate...");

            await _requestGate.Close();

            _logger.Debug("Closing connection...");

            await _core.Connection.Disconnect();

            _lostConnection = false;
            ScheduleReconnect(false);
            ChangeState(AccountModelInfo.Types.ConnectionState.Offline);

            _logger.Debug("Offline!");

            _shutdownCompletionSrc?.TrySetResult(null);

            ManageConnectionInternal();
        }


        private void ChangeState(AccountModelInfo.Types.ConnectionState newState)
        {
            LogConnectionState(_state, newState);
            _state = newState;
            //StateChanged?.Invoke(this);
        }

        private void LogConnectionState(AccountModelInfo.Types.ConnectionState oldState, AccountModelInfo.Types.ConnectionState newState)
        {
            var server = _savedState.Server;
            var userId = _savedState.UserId;

            if (IsConnected(oldState, newState))
                _logger.Info($"{userId}: login on {server}");
            else if (!HasError() && IsUsualDisconnect(oldState, newState))
                _logger.Info($"{userId}: logout from {server}");
            else if (HasError() && IsFailedConnection(oldState, newState))
                _logger.Info($"{userId}: connect to {server} failed [{_lastError?.Code}]");
            else if (HasError() && IsUnexpectedDisconnect(oldState, newState))
                _logger.Info($"{userId}: connection to {server} lost [{_lastError?.Code}]");
        }


        private bool HasError() => _lastError != null && !_lastError.IsOk;

        private static bool IsConnected(AccountModelInfo.Types.ConnectionState from, AccountModelInfo.Types.ConnectionState to)
            => to == AccountModelInfo.Types.ConnectionState.Online;

        private static bool IsUsualDisconnect(AccountModelInfo.Types.ConnectionState from, AccountModelInfo.Types.ConnectionState to)
            => from == AccountModelInfo.Types.ConnectionState.Disconnecting && to == AccountModelInfo.Types.ConnectionState.Offline;

        private static bool IsUnexpectedDisconnect(AccountModelInfo.Types.ConnectionState from, AccountModelInfo.Types.ConnectionState to)
            => from == AccountModelInfo.Types.ConnectionState.Online && (to == AccountModelInfo.Types.ConnectionState.Offline || to == AccountModelInfo.Types.ConnectionState.Disconnecting);

        private static bool IsFailedConnection(AccountModelInfo.Types.ConnectionState from, AccountModelInfo.Types.ConnectionState to)
            => from == AccountModelInfo.Types.ConnectionState.Connecting && to == AccountModelInfo.Types.ConnectionState.Offline;


        internal sealed class ShutdownCmd : Singleton<ShutdownCmd> { }

        private sealed class ManageConnectionCmd : Singleton<ManageConnectionCmd> { }

        private sealed class ConnectionLostMsg : Singleton<ConnectionLostMsg> { }

        private sealed class ScheduleDisconnectCmd : Singleton<ScheduleDisconnectCmd> { }
    }
}