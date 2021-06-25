using System.Collections.Generic;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Server
{
    internal class AccountConsumerController : Actor
    {
        private readonly IAccountProxy _acc;
        private readonly string _accId;
        private readonly Dictionary<string, RpcSession> _sessions = new Dictionary<string, RpcSession>();

        private IAlgoLogger _logger;
        private int _refCnt;


        private AccountConsumerController(IAccountProxy acc)
        {
            _acc = acc;
            _accId = acc.Id;

            Receive<AttachSessionCmd>(AttachSession);
            Receive<DetachSessionCmd>(DetachSession);
            Receive<RpcMessage>(SendNotification);
        }


        public static IActorRef Create(IAccountProxy acc)
        {
            return ActorSystem.SpawnLocal(() => new AccountConsumerController(acc), $"{nameof(AccountConsumerController)} {acc.Id}");
        }


        protected override void ActorInit(object initMsg)
        {
            _logger = AlgoLoggerFactory.GetLogger(Name);
        }


        private void AttachSession(AttachSessionCmd cmd)
        {
            var session = cmd.Session;
            var sessionId = session.Id;

            _logger.Debug($"Attaching session {sessionId}...");

            _sessions[sessionId] = session;

            _refCnt++;
            if (_refCnt == 1)
            {
                _acc.AccInfoProvider.OrderUpdated += OnOrderUpdated;
                _acc.AccInfoProvider.PositionUpdated += OnPositionUpdated;
                _acc.AccInfoProvider.BalanceUpdated += OnBalanceUpdated;

                _acc.Feed.RateUpdated += OnRateUpdated;
                _acc.Feed.RatesUpdated += OnRatesUpdated;
            }

            _logger.Debug($"Attached session {sessionId}. Have {_refCnt} active refs");
        }

        private void DetachSession(DetachSessionCmd cmd)
        {
            var sessionId = cmd.SessionId;

            _logger.Debug($"Detaching session {sessionId}...");

            _refCnt--;
            if (_refCnt == 0)
            {
                _acc.AccInfoProvider.OrderUpdated -= OnOrderUpdated;
                _acc.AccInfoProvider.PositionUpdated -= OnPositionUpdated;
                _acc.AccInfoProvider.BalanceUpdated -= OnBalanceUpdated;

                _acc.Feed.RateUpdated -= OnRateUpdated;
                _acc.Feed.RatesUpdated -= OnRatesUpdated;
            }

            _logger.Debug($"Detached session {sessionId}. Have {_refCnt} active refs");
        }

        private IAccountProxy GetAccountProxy(AccountProxyRequest request) => _acc;


        private void SendNotification(RpcMessage msg)
        {
            if (_sessions.Count == 0)
                return;

            foreach (var session in _sessions.Values)
            {
                session.Tell(msg);
            }
        }


        private void OnOrderUpdated(OrderExecReport r) => Self.Tell(RpcMessage.Notification(_accId, r));

        private void OnPositionUpdated(PositionExecReport r) => Self.Tell(RpcMessage.Notification(_accId, r));

        private void OnBalanceUpdated(BalanceOperation r) => Self.Tell(RpcMessage.Notification(_accId, r));

        private void OnRateUpdated(QuoteInfo r) => Self.Tell(RpcMessage.Notification(_accId, r.GetFullQuote()));

        private void OnRatesUpdated(List<QuoteInfo> r) => Self.Tell(RpcMessage.Notification(_accId, QuotePage.Create(r)));


        internal class AttachSessionCmd
        {
            public RpcSession Session { get; }

            public AttachSessionCmd(RpcSession session)
            {
                Session = session;
            }
        }

        internal class DetachSessionCmd
        {
            public string SessionId { get; }

            public DetachSessionCmd(string sessionId)
            {
                SessionId = sessionId;
            }
        }

        internal class AccountProxyRequest { }
    }
}
