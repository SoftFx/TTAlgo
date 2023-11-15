using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using TickTrader.Algo.Async;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Server
{
    public class AccountRpcNotification
    {
        private RpcMessage _msg;


        public string Id { get; }

        public IMessage Payload { get; }

        public RpcMessage Message => _msg ??= RpcMessage.Notification(Id, Payload);


        public AccountRpcNotification(string id, IMessage payload)
        {
            Id = id;
            Payload = payload;
        }
    }


    // Not thread safe. Requires actor context
    public class AccountRpcController
    {
        private readonly Dictionary<string, AccountRpcHandler> _sessionHandlers = new();
        private readonly IAlgoLogger _logger;
        private readonly string _id;

        private IAccountProxy _accProxy;
        private Channel<AccountRpcNotification> _notificationBus;

        public int RefCnt { get; private set; }


        public AccountRpcController(IAlgoLogger logger, string accId)
        {
            _logger = logger;
            _id = accId;
        }


        public void SetAccountProxy(IAccountProxy accProxy)
        {
            if (RefCnt != 0)
                throw new AlgoException("Can't change acc proxy when there are active refs");

            _accProxy = accProxy;
        }

        public void OnConnectionStateUpdate(ConnectionStateUpdate update) => PushNotification(new AccountRpcNotification(_id, update));

        public AccountRpcHandler AttachSession(RpcSession session, Domain.Account.Types.ConnectionState currentState)
        {
            var sessionId = session.Id;

            _logger.Debug($"Attaching session {sessionId}...");

            var sessionHandler = new AccountRpcHandler(_accProxy, session, _id);
            _sessionHandlers[sessionId] = sessionHandler;

            var update = new ConnectionStateUpdate(_id, currentState, currentState);
            sessionHandler.DispatchNotification(new AccountRpcNotification(_id, update));

            RefCnt++;
            if (RefCnt == 1)
            {
                _notificationBus = DefaultChannelFactory.CreateForManyToOne<AccountRpcNotification>();
                _ = _notificationBus.Consume(DispatchNotification, 8);

                var acc = _accProxy;
                acc.AccInfoProvider.OrderUpdated += OnOrderUpdated;
                acc.AccInfoProvider.PositionUpdated += OnPositionUpdated;
                acc.AccInfoProvider.BalanceUpdated += OnBalanceUpdated;

                acc.Feed.QuoteUpdated += OnQuoteUpdated;
                acc.Feed.BarUpdated += OnBarUpdated;
            }

            _logger.Debug($"Attached session {sessionId}. Have {RefCnt} active refs");

            return sessionHandler;
        }

        public void DetachSession(string sessionId)
        {
            _logger.Debug($"Detaching session {sessionId}...");

            var sessionHandler = _sessionHandlers[sessionId];
            sessionHandler.Dispose();
            _sessionHandlers.Remove(sessionId);

            RefCnt--;
            if (RefCnt == 0)
            {
                _notificationBus.Writer.TryComplete();
                _notificationBus = null;

                var acc = _accProxy;
                acc.AccInfoProvider.OrderUpdated -= OnOrderUpdated;
                acc.AccInfoProvider.PositionUpdated -= OnPositionUpdated;
                acc.AccInfoProvider.BalanceUpdated -= OnBalanceUpdated;

                acc.Feed.QuoteUpdated -= OnQuoteUpdated;
                acc.Feed.BarUpdated -= OnBarUpdated;
            }

            _logger.Debug($"Detached session {sessionId}. Have {RefCnt} active refs");
        }


        private void OnOrderUpdated(OrderExecReport r) => PushNotification(new AccountRpcNotification(_id, r)); // not actor thread. _id is safe since it is readonly

        private void OnPositionUpdated(PositionExecReport r) => PushNotification(new AccountRpcNotification(_id, r)); // not actor thread. _id is safe since it is readonly

        private void OnBalanceUpdated(BalanceOperation r) => PushNotification(new AccountRpcNotification(_id, r)); // not actor thread. _id is safe since it is readonly

        private void OnQuoteUpdated(QuoteInfo r) => PushNotification(new AccountRpcNotification(_id, r.GetFullQuote())); // not actor thread. _id is safe since it is readonly

        private void OnBarUpdated(BarUpdate b) => PushNotification(new AccountRpcNotification(_id, b)); // not actor thread. _id is safe since it is readonly

        private void PushNotification(AccountRpcNotification msg) => _notificationBus?.Writer.TryWrite(msg);

        private void DispatchNotification(AccountRpcNotification msg)
        {
            if (_sessionHandlers.Count == 0)
                return;

            var cleanupSessions = false;
            foreach (var handler in _sessionHandlers.Values)
            {
                try
                {
                    if (handler.SessionState == RpcSessionState.Connected)
                    {
                        handler.DispatchNotification(msg);
                    }
                    else
                    {
                        cleanupSessions = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Failed to send message to session {handler.SessionId}");
                }
            }

            if (cleanupSessions)
            {
                try
                {
                    var sessionsIdToRemove = _sessionHandlers.Where(s => s.Value.SessionState != RpcSessionState.Connected).Select(s => s.Key).ToList();
                    foreach (var sessionId in sessionsIdToRemove)
                    {
                        DetachSession(sessionId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to cleanup disconnected sessions");
                }
            }
        }
    }
}
