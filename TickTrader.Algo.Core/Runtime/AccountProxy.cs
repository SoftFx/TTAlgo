using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Infrastructure;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Core
{
    public interface IAccountProxy
    {
        string Id { get; }

        IAccountInfoProvider AccInfoProvider { get; }

        ITradeHistoryProvider TradeHistoryProvider { get; }

        IPluginMetadata Metadata { get; }

        IFeedProvider Feed { get; }

        IFeedHistoryProvider FeedHistory { get; }

        ITradeExecutor TradeExecutor { get; }
    }


    public class LocalAccountProxy : IAccountProxy
    {
        public string Id { get; }

        public IAccountInfoProvider AccInfoProvider { get; set; }

        public ITradeHistoryProvider TradeHistoryProvider { get; set; }

        public IPluginMetadata Metadata { get; set; }

        public IFeedProvider Feed { get; set; }

        public IFeedHistoryProvider FeedHistory { get; set; }

        public ITradeExecutor TradeExecutor { get; set; }


        public LocalAccountProxy(string id)
        {
            Id = id;
        }
    }


    public class RemoteAccountProxy : IAccountProxy, IRpcHandler
    {
        private readonly RuntimeInfoProvider _provider;
        private readonly QuoteDistributor _distributor;

        private RpcSession _session;
        private int _refCnt;


        public string Id { get; }

        public IAccountInfoProvider AccInfoProvider => _provider;

        public ITradeHistoryProvider TradeHistoryProvider => _provider;

        public IPluginMetadata Metadata => _provider;

        public IFeedProvider Feed => _provider;

        public IFeedHistoryProvider FeedHistory => _provider;

        public ITradeExecutor TradeExecutor => _provider;

        public event Action<OrderExecReport> OrderUpdated;
        public event Action<PositionExecReport> PositionUpdated;
        public event Action<BalanceOperation> BalanceUpdated;

        public event Action<QuoteInfo> RateUpdated;
        public event Action<List<QuoteInfo>> RatesUpdated;


        public RemoteAccountProxy(string id)
        {
            Id = id;

            _provider = new RuntimeInfoProvider(this);
            _distributor = new QuoteDistributor();
        }


        void IRpcHandler.SetSession(RpcSession session)
        {
            _session = session;
        }

        void IRpcHandler.HandleNotification(string proxyId, string callId, Any payload)
        {
            if (payload.Is(OrderExecReport.Descriptor))
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


        internal async Task AddRef()
        {
            _refCnt++;
            if (_refCnt == 1)
            {
                await Start();
            }
        }

        internal async Task RemoveRef()
        {
            _refCnt--;
            if (_refCnt == 0)
            {
                await Stop();
            }
        }
        
        private async Task Start()
        {
            await _provider.PreLoad();
            await Task.Factory.StartNew(() => _distributor.Start(_provider));
        }

        private async Task Stop()
        {
            await Task.Factory.StartNew(() => _distributor.Stop());
        }


        internal IFeedSubscription GetSubscription()
        {
            return _distributor.AddSubscription(q => { });
        }

        internal List<CurrencyInfo> GetCurrencyList()
        {
            return GetCurrencyListAsync().GetAwaiter().GetResult();
        }

        internal List<SymbolInfo> GetSymbolList()
        {
            return GetSymbolListAsync().GetAwaiter().GetResult();
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

        internal AccountInfo GetAccountInfo()
        {
            return GetAccountInfoAsync().GetAwaiter().GetResult();
        }

        internal async Task<AccountInfo> GetAccountInfoAsync()
        {
            var context = new RpcResponseTaskContext<AccountInfoResponse>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(Id, new AccountInfoRequest()), context);
            var res = await context.TaskSrc.Task;
            return res.Account;
        }

        internal List<OrderInfo> GetOrderList()
        {
            return GetOrderListAsync().GetAwaiter().GetResult();
        }

        internal Task<List<OrderInfo>> GetOrderListAsync()
        {
            var context = new RpcListResponseTaskContext<OrderInfo>(OrderListReponseHandler);
            _session.Ask(RpcMessage.Request(Id, new OrderListRequest()), context);
            return context.TaskSrc.Task;
        }

        internal List<PositionInfo> GetPositionList()
        {
            return GetPositionListAsync().GetAwaiter().GetResult();
        }

        internal Task<List<PositionInfo>> GetPositionListAsync()
        {
            var context = new RpcListResponseTaskContext<PositionInfo>(PositionListReponseHandler);
            _session.Ask(RpcMessage.Request(Id, new PositionListRequest()), context);
            return context.TaskSrc.Task;
        }

        internal void SendOpenOrder(OpenOrderRequest request)
        {
            var context = new RpcResponseTaskContext<VoidResponse>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(Id, request), context);
            context.TaskSrc.Task.GetAwaiter().GetResult();
        }

        internal void SendModifyOrder(ModifyOrderRequest request)
        {
            var context = new RpcResponseTaskContext<VoidResponse>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(Id, request), context);
            context.TaskSrc.Task.GetAwaiter().GetResult();
        }

        internal void SendCloseOrder(CloseOrderRequest request)
        {
            var context = new RpcResponseTaskContext<VoidResponse>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(Id, request), context);
            context.TaskSrc.Task.GetAwaiter().GetResult();
        }

        internal void SendCancelOrder(CancelOrderRequest request)
        {
            var context = new RpcResponseTaskContext<VoidResponse>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(Id, request), context);
            context.TaskSrc.Task.GetAwaiter().GetResult();
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

        internal async Task<List<QuoteInfo>> GetFeedSnapshotAsync()
        {
            var context = new RpcResponseTaskContext<QuotePage>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(Id, new FeedSnapshotRequest()), context);
            var res = await context.TaskSrc.Task;
            return res.Quotes.Select(q => new QuoteInfo(q)).ToList();
        }

        internal List<QuoteInfo> ModifyFeedSubscription(ModifyFeedSubscriptionRequest request)
        {
            return ModifyFeedSubscriptionAsync(request).GetAwaiter().GetResult();
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
            var report = payload.Unpack<Domain.OrderExecReport>();
            OrderUpdated?.Invoke(report);
        }

        private void PositionExecReportNotificationHandler(Any payload)
        {
            var report = payload.Unpack<Domain.PositionExecReport>();
            PositionUpdated?.Invoke(report);
        }

        private void BalanceOperationNotificationHandler(Any payload)
        {
            var report = payload.Unpack<BalanceOperation>();
            BalanceUpdated?.Invoke(report);
        }

        private void FullQuoteInfoNotificationHandler(Any payload)
        {
            var quote = payload.Unpack<FullQuoteInfo>();
            RateUpdated?.Invoke(new QuoteInfo(quote));
        }

        private void QuotePageNotificationHandler(Any payload)
        {
            var page = payload.Unpack<QuotePage>();
            RatesUpdated?.Invoke(page.Quotes.Select(q => new QuoteInfo(q)).ToList());
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

        private bool TradeHistoryPageResponseHandler(TaskCompletionSource<Domain.TradeReportInfo[]> taskSrc, Any payload)
        {
            if (payload.TryGetError(out var ex))
            {
                taskSrc.TrySetException(ex);
            }
            else
            {
                var response = payload.Unpack<TradeHistoryPageResponse>();
                var res = new Domain.TradeReportInfo[response.Reports.Count];
                response.Reports.CopyTo(res, 0);
                taskSrc.TrySetResult(res);
                if (res.Length != 0)
                    return false;
            }

            return true;
        }


        private class PagedEnumeratorAdapter<T> : IAsyncPagedEnumerator<T>, IRpcResponseContext
        {
            private TaskCompletionSource<T[]> _pageTaskSrc;

            Func<TaskCompletionSource<T[]>, Any, bool> ResponseHandler { get; }

            Action GetNextPageHandler { get; }

            Action DisposeHandler { get; }


            public PagedEnumeratorAdapter(Func<TaskCompletionSource<T[]>, Any, bool> responseHandler, Action getNextPageHandler, Action disposeHandler)
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

            public Task<T[]> GetNextPage()
            {
                if (_pageTaskSrc != null)
                    throw new Exception("Can't get more than one page at a time");

                _pageTaskSrc = new TaskCompletionSource<T[]>();
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
