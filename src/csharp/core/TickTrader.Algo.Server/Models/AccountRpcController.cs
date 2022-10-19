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
    // Not thread safe. Requires actor context
    public class AccountRpcController
    {
        internal record AttachSessionCmd(RpcSession Session);

        internal record DetachSessionCmd(string SessionId);


        private readonly Dictionary<string, RpcSession> _sessions = new();
        private readonly Dictionary<string, AccountRpcHandler> _sessionHandlers = new();
        private readonly IAlgoLogger _logger;
        private readonly string _id;

        private IAccountProxy _accProxy;
        private Channel<RpcMessage> _notificationBus;

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

        public void OnConnectionStateUpdate(ConnectionStateUpdate update) => PushNotification(RpcMessage.Notification(_id, update));

        public AccountRpcHandler AttachSession(RpcSession session, Domain.Account.Types.ConnectionState currentState)
        {
            var sessionId = session.Id;

            _logger.Debug($"Attaching session {sessionId}...");

            _sessions[sessionId] = session;

            var sessionHandler = new AccountRpcHandler(_accProxy, session);
            _sessionHandlers[sessionId] = sessionHandler;

            var update = new ConnectionStateUpdate(_id, currentState, currentState);
            session.Tell(RpcMessage.Notification(_id, update));

            RefCnt++;
            if (RefCnt == 1)
            {
                _notificationBus = DefaultChannelFactory.CreateForManyToOne<RpcMessage>();
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

            _sessions.Remove(sessionId);

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
                acc.Feed.BarUpdated += OnBarUpdated;
            }

            _logger.Debug($"Detached session {sessionId}. Have {RefCnt} active refs");
        }


        private void OnOrderUpdated(OrderExecReport r) => PushNotification(RpcMessage.Notification(_id, r)); // not actor thread. _id is safe since it is readonly

        private void OnPositionUpdated(PositionExecReport r) => PushNotification(RpcMessage.Notification(_id, r)); // not actor thread. _id is safe since it is readonly

        private void OnBalanceUpdated(BalanceOperation r) => PushNotification(RpcMessage.Notification(_id, r)); // not actor thread. _id is safe since it is readonly

        private void OnQuoteUpdated(QuoteInfo r) => PushNotification(RpcMessage.Notification(_id, r.GetFullQuote())); // not actor thread. _id is safe since it is readonly

        private void OnBarUpdated(BarUpdate b) => PushNotification(RpcMessage.Notification(_id, b)); // not actor thread. _id is safe since it is readonly

        private void PushNotification(RpcMessage msg) => _notificationBus?.Writer.TryWrite(msg);

        private void DispatchNotification(RpcMessage msg)
        {
            if (_sessions.Count == 0)
                return;

            var cleanupSessions = false;
            foreach (var session in _sessions.Values)
            {
                try
                {
                    if (session.State == RpcSessionState.Connected)
                    {
                        session.Tell(msg);
                    }
                    else
                    {
                        cleanupSessions = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Failed to send message to session {session.Id}");
                }
            }

            if (cleanupSessions)
            {
                try
                {
                    var sessionsIdToRemove = _sessions.Where(s => s.Value.State != RpcSessionState.Connected).Select(s => s.Key).ToList();
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
