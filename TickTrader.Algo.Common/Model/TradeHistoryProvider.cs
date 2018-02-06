using ActorSharp;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Common.Model
{
    public class TradeHistoryProvider : ActorPart
    {
        private ConnectionModel _connection;

        public TradeHistoryProvider(ConnectionModel connection)
        {
            _connection = connection;
        }

        public class Handler : CrossDomainObject, ITradeHistoryProvider
        {
            private Ref<TradeHistoryProvider> _ref;

            public Handler(Ref<TradeHistoryProvider> actorRef)
            {
                _ref = actorRef;
            }

            public IAsyncEnumerator<TradeReportEntity[]> GetTradeHistory(bool skipCancelOrders)
            {
                return GetTradeHistoryInternal(null, null, skipCancelOrders);
            }

            public IAsyncEnumerator<TradeReportEntity[]> GetTradeHistory(DateTime from, DateTime to, bool skipCancelOrders)
            {
                return GetTradeHistoryInternal(from, to, skipCancelOrders);
            }

            public IAsyncEnumerator<TradeReportEntity[]> GetTradeHistory(DateTime to, bool skipCancelOrders)
            {
                return GetTradeHistoryInternal(null, to, skipCancelOrders);
            }

            IAsyncCrossDomainEnumerator<TradeReport> ITradeHistoryProvider.GetTradeHistory(bool skipCancelOrders)
            {
                return GetTradeHistory(skipCancelOrders).Select(r => (TradeReport[])r).AsCrossDomain();
            }

            IAsyncCrossDomainEnumerator<TradeReport> ITradeHistoryProvider.GetTradeHistory(DateTime from, DateTime to, bool skipCancelOrders)
            {
                return GetTradeHistory(from, to, skipCancelOrders).Select(r => (TradeReport[])r).AsCrossDomain();
            }

            IAsyncCrossDomainEnumerator<TradeReport> ITradeHistoryProvider.GetTradeHistory(DateTime to, bool skipCancelOrders)
            {
                return GetTradeHistory(to, skipCancelOrders).Select(r => (TradeReport[])r).AsCrossDomain();
            }

            private IAsyncEnumerator<TradeReportEntity[]> GetTradeHistoryInternal(DateTime? from, DateTime? to, bool skipCancelOrders)
            {
                throw new NotImplementedException();
                //return _ref.Call(a => a._connection.TradeProxy.GetTradeHistory(from, to, skipCancelOrders));
            }
        }
    }
}
