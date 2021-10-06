using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Async;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Infrastructure;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Runtime
{
    public class RemoteAccountProxy : IRpcHandler, IFeedSubscription
    {
        private readonly IActorRef _acc;
        private readonly QuoteDistributor _distributor;

        private RpcSession _session;
        private TaskCompletionSource<object> _startTaskSrc, _stopTaskSrc;
        private readonly ChannelEventSource<OrderExecReport> _orderEventSrc = new ChannelEventSource<OrderExecReport>();
        private readonly ChannelEventSource<PositionExecReport> _positionEventSrc = new ChannelEventSource<PositionExecReport>();
        private readonly ChannelEventSource<BalanceOperation> _balanceEventSrc = new ChannelEventSource<BalanceOperation>();
        private readonly ChannelEventSource<QuoteInfo> _rateEventSrc = new ChannelEventSource<QuoteInfo>();
        private readonly ChannelEventSource<List<QuoteInfo>> _rateListEventSrc = new ChannelEventSource<List<QuoteInfo>>();


        public string Id { get; }

        public IEventSource<OrderExecReport> OrderUpdated => _orderEventSrc;

        public IEventSource<PositionExecReport> PositionUpdated => _positionEventSrc;

        public IEventSource<BalanceOperation> BalanceUpdated => _balanceEventSrc;

        public IEventSource<QuoteInfo> RateUpdated => _rateEventSrc;

        public IEventSource<List<QuoteInfo>> RatesUpdated => _rateListEventSrc;


        public RemoteAccountProxy(string id, IActorRef acc)
        {
            Id = id;
            _acc = acc;

            _distributor = new QuoteDistributor();
        }


        void IRpcHandler.SetSession(RpcSession session)
        {
            _session = session;
        }

        void IRpcHandler.HandleNotification(string proxyId, string callId, Any payload)
        {
            if (payload.Is(ConnectionStateUpdate.Descriptor))
                _acc.Tell(payload.Unpack<ConnectionStateUpdate>());
            else if (payload.Is(OrderExecReport.Descriptor))
                OrderExecReportNotificationHandler(payload);
            else if (payload.Is(PositionExecReport.Descriptor))
                PositionExecReportNotificationHandler(payload);
            else if (payload.Is(BalanceOperation.Descriptor))
                BalanceOperationNotificationHandler(payload);
            else if (payload.Is(FullQuoteInfo.Descriptor))
                FullQuoteInfoNotificationHandler(payload);
            else if (payload.Is(QuotePage.Descriptor))
                QuotePageNotificationHandler(payload);
        }

        Task<Any> IRpcHandler.HandleRequest(string proxyId, string callId, Any payload)
        {
            throw new NotSupportedException($"{nameof(RemoteAccountProxy)} doesn't support incoming requests");
        }


        List<QuoteInfo> IFeedSubscription.Modify(List<FeedSubscriptionUpdate> updates)
        {
            return (this as IFeedSubscription).ModifyAsync(updates).GetAwaiter().GetResult();
        }

        Task<List<QuoteInfo>> IFeedSubscription.ModifyAsync(List<FeedSubscriptionUpdate> updates)
        {
            var request = new ModifyFeedSubscriptionRequest();
            request.Updates.AddRange(updates);
            return ModifyFeedSubscriptionAsync(request);
        }

        void IFeedSubscription.CancelAll()
        {
            (this as IFeedSubscription).CancelAllAsync().GetAwaiter().GetResult();
        }

        Task IFeedSubscription.CancelAllAsync()
        {
            return CancelAllFeedSubscriptionsAsync();
        }


        public async Task Start()
        {
            if (_stopTaskSrc != null)
                await _stopTaskSrc.Task;

            _startTaskSrc = new TaskCompletionSource<object>();
            try
            {
                await Task.Factory.StartNew(() => _distributor.Start(this));
            }
            finally
            {
                _startTaskSrc.TrySetResult(null);
                _startTaskSrc = null;
            }
        }

        public async Task Stop()
        {
            if (_startTaskSrc != null)
                await _startTaskSrc.Task;

            _stopTaskSrc = new TaskCompletionSource<object>();
            try
            {
                await Task.Factory.StartNew(() => _distributor.Stop());
            }
            finally
            {
                _stopTaskSrc?.TrySetResult(null);
                _stopTaskSrc = null;
            }
        }

        public void Dispose()
        {
            _orderEventSrc.Dispose();
            _positionEventSrc.Dispose();
            _balanceEventSrc.Dispose();
            _rateEventSrc.Dispose();
            _rateListEventSrc.Dispose();
        }


        internal IFeedSubscription GetSubscription()
        {
            return _distributor.AddSubscription(q => { });
        }

        internal async Task<List<CurrencyInfo>> GetCurrencyListAsync()
        {
            var context = new RpcResponseTaskContext<CurrencyListResponse>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(Id, new CurrencyListRequest()), context);
            var res = await context.TaskSrc.Task;
            return res.Currencies.ToList();
        }

        internal async Task<List<SymbolInfo>> GetSymbolListAsync()
        {
            var context = new RpcResponseTaskContext<SymbolListResponse>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(Id, new SymbolListRequest()), context);
            var res = await context.TaskSrc.Task;
            return res.Symbols.ToList();
        }

        internal async Task<List<FullQuoteInfo>> GetLastQuotesListAsync()
        {
            var context = new RpcResponseTaskContext<LastQuoteListResponse>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(Id, new LastQuoteListRequest()), context);
            var res = await context.TaskSrc.Task;
            return res.Quotes.ToList();
        }

        internal async Task<AccountInfo> GetAccountInfoAsync()
        {
            var context = new RpcResponseTaskContext<AccountInfoResponse>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(Id, new AccountInfoRequest()), context);
            var res = await context.TaskSrc.Task;
            return res.Account;
        }

        internal Task<List<OrderInfo>> GetOrderListAsync()
        {
            var context = new RpcListResponseTaskContext<OrderInfo>(OrderListReponseHandler);
            _session.Ask(RpcMessage.Request(Id, new OrderListRequest()), context);
            return context.TaskSrc.Task;
        }

        internal Task<List<PositionInfo>> GetPositionListAsync()
        {
            var context = new RpcListResponseTaskContext<PositionInfo>(PositionListReponseHandler);
            _session.Ask(RpcMessage.Request(Id, new PositionListRequest()), context);
            return context.TaskSrc.Task;
        }

        internal void SendOpenOrder(OpenOrderRequest request)
        {
            _session.Tell(RpcMessage.Request(Id, request));
        }

        internal void SendModifyOrder(ModifyOrderRequest request)
        {
            _session.Tell(RpcMessage.Request(Id, request));
        }

        internal void SendCloseOrder(CloseOrderRequest request)
        {
            _session.Tell(RpcMessage.Request(Id, request));
        }

        internal void SendCancelOrder(CancelOrderRequest request)
        {
            _session.Tell(RpcMessage.Request(Id, request));
        }

        internal IAsyncPagedEnumerator<TradeReportInfo> GetTradeHistory(TradeHistoryRequest request)
        {
            var callId = RpcMessage.GenerateCallId();
            var context = new PagedEnumeratorAdapter<TradeReportInfo>(TradeHistoryPageResponseHandler,
                () => _session.Tell(RpcMessage.Request(Id, callId, new TradeHistoryRequestNextPage())),
                () => _session.Tell(RpcMessage.Request(Id, callId, new TradeHistoryRequestDispose())));
            _session.Ask(RpcMessage.Request(Id, callId, request), context);
            return context;
        }

        internal IAsyncPagedEnumerator<TriggerReportInfo> GetTriggerHistory(TriggerHistoryRequest request)
        {
            var callId = RpcMessage.GenerateCallId();
            var context = new PagedEnumeratorAdapter<TriggerReportInfo>(TriggerHistoryPageResponseHandler,
                () => _session.Tell(RpcMessage.Request(Id, callId, new TriggerHistoryRequestNextPage())),
                () => _session.Tell(RpcMessage.Request(Id, callId, new TriggerHistoryRequestDispose())));
            _session.Ask(RpcMessage.Request(Id, callId, request), context);
            return context;
        }

        internal async Task<List<QuoteInfo>> GetFeedSnapshotAsync()
        {
            var context = new RpcResponseTaskContext<QuotePage>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(Id, new FeedSnapshotRequest()), context);
            var res = await context.TaskSrc.Task;
            return res.Quotes.Select(q => new QuoteInfo(q)).ToList();
        }

        internal async Task<List<QuoteInfo>> ModifyFeedSubscriptionAsync(ModifyFeedSubscriptionRequest request)
        {
            var context = new RpcResponseTaskContext<QuotePage>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(Id, request), context);
            var res = await context.TaskSrc.Task.ConfigureAwait(false);
            return res.Quotes.Select(q => new QuoteInfo(q)).ToList();
        }

        internal Task CancelAllFeedSubscriptionsAsync()
        {
            var context = new RpcResponseTaskContext<VoidResponse>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(Id, new CancelAllFeedSubscriptionsRequest()), context);
            return context.TaskSrc.Task;
        }

        internal async Task<List<BarData>> GetBarListAsync(BarListRequest request)
        {
            var context = new RpcResponseTaskContext<BarChunk>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(Id, request), context);
            var res = await context.TaskSrc.Task;
            return res.Bars.ToList();
        }

        internal async Task<List<QuoteInfo>> GetQuoteListAsync(QuoteListRequest request)
        {
            var context = new RpcResponseTaskContext<QuoteChunk>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(Id, request), context);
            var res = await context.TaskSrc.Task;
            var symbol = res.Symbol;
            return res.Quotes.Select(q => new QuoteInfo(symbol, q)).ToList();
        }

        private void OrderExecReportNotificationHandler(Any payload)
        {
            _orderEventSrc.Send(payload.Unpack<OrderExecReport>());
        }

        private void PositionExecReportNotificationHandler(Any payload)
        {
            _positionEventSrc.Send(payload.Unpack<PositionExecReport>());
        }

        private void BalanceOperationNotificationHandler(Any payload)
        {
            _balanceEventSrc.Send(payload.Unpack<BalanceOperation>());
        }

        private void FullQuoteInfoNotificationHandler(Any payload)
        {
            var quote = payload.Unpack<FullQuoteInfo>();
            _rateEventSrc.Send(new QuoteInfo(quote));
        }

        private void QuotePageNotificationHandler(Any payload)
        {
            var page = payload.Unpack<QuotePage>();
            _rateListEventSrc.Send(page.Quotes.Select(q => new QuoteInfo(q)).ToList());
        }


        private bool OrderListReponseHandler(IObserver<RepeatedField<OrderInfo>> observer, Any payload)
        {
            if (payload.TryGetError(out var ex))
            {
                observer.OnError(ex);
            }
            else
            {
                var response = payload.Unpack<OrderListResponse>();
                observer.OnNext(response.Orders);
                if (!response.IsFinal)
                    return false;

                observer.OnCompleted();
            }

            return true;
        }

        private bool PositionListReponseHandler(IObserver<RepeatedField<PositionInfo>> observer, Any payload)
        {
            if (payload.TryGetError(out var ex))
            {
                observer.OnError(ex);
            }
            else
            {
                var response = payload.Unpack<PositionListResponse>();
                observer.OnNext(response.Positions);
                if (!response.IsFinal)
                    return false;

                observer.OnCompleted();
            }

            return true;
        }

        private bool TradeHistoryPageResponseHandler(TaskCompletionSource<List<TradeReportInfo>> taskSrc, Any payload)
        {
            if (payload.TryGetError(out var ex))
            {
                taskSrc.TrySetException(ex);
            }
            else
            {
                var response = payload.Unpack<TradeHistoryPageResponse>();
                var res = new List<TradeReportInfo>(response.Reports);

                taskSrc.TrySetResult(res);

                if (response.Reports.Count != 0)
                    return false;
            }

            return true;
        }

        private bool TriggerHistoryPageResponseHandler(TaskCompletionSource<List<TriggerReportInfo>> taskSrc, Any payload)
        {
            if (payload.TryGetError(out var ex))
            {
                taskSrc.TrySetException(ex);
            }
            else
            {
                var response = payload.Unpack<TriggerHistoryPageResponse>();
                var res = new List<TriggerReportInfo>(response.Reports);

                taskSrc.TrySetResult(res);

                if (response.Reports.Count != 0)
                    return false;
            }

            return true;
        }

        private class PagedEnumeratorAdapter<T> : IAsyncPagedEnumerator<T>, IRpcResponseContext
        {
            private TaskCompletionSource<List<T>> _pageTaskSrc;

            Func<TaskCompletionSource<List<T>>, Any, bool> ResponseHandler { get; }

            Action GetNextPageHandler { get; }

            Action DisposeHandler { get; }


            public PagedEnumeratorAdapter(Func<TaskCompletionSource<List<T>>, Any, bool> responseHandler, Action getNextPageHandler, Action disposeHandler)
            {
                ResponseHandler = responseHandler;
                GetNextPageHandler = getNextPageHandler;
                DisposeHandler = disposeHandler;
            }


            public void Dispose()
            {
                _pageTaskSrc?.TrySetCanceled();
                DisposeHandler();
            }

            public Task<List<T>> GetNextPage()
            {
                if (_pageTaskSrc != null)
                    throw new Exception("Can't get more than one page at a time");

                _pageTaskSrc = new TaskCompletionSource<List<T>>();
                var res = _pageTaskSrc.Task;
                GetNextPageHandler();
                return res;
            }

            public bool OnNext(Any payload)
            {
                var taskSrc = _pageTaskSrc;
                _pageTaskSrc = null;
                return ResponseHandler(taskSrc, payload);
            }
        }
    }
}
