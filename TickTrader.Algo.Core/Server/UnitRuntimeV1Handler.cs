using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Core
{
    internal class UnitRuntimeV1Handler : IRpcHandler, IPluginMetadata
    {
        private readonly PluginExecutorCore _executorCore;
        private RpcSession _session;


        public event Action<Domain.OrderExecReport> OrderUpdated;
        public event Action<Domain.PositionExecReport> PositionUpdated;
        public event Action<BalanceOperation> BalanceUpdated;


        public UnitRuntimeV1Handler(PluginExecutorCore executorCore)
        {
            _executorCore = executorCore;

            executorCore.OnNotification = msg => _session.Tell(RpcMessage.Notification(msg));
        }


        public Task<bool> AttachPlugin(string executorId)
        {
            var context = new RpcResponseTaskContext<bool>(AttachPluginResponseHandler);
            _session.Ask(RpcMessage.Request(new AttachPluginRequest { Id = executorId }), context);
            return context.TaskSrc.Task;
        }


        IEnumerable<CurrencyInfo> IPluginMetadata.GetCurrencyMetadata()
        {
            var context = new RpcResponseTaskContext<CurrencyListResponse>(SingleReponseHandler);
            _session.Ask(RpcMessage.Request(new CurrencyListRequest()), context);
            var res = context.TaskSrc.Task.GetAwaiter().GetResult();
            return res.Currencies;
        }

        IEnumerable<SymbolInfo> IPluginMetadata.GetSymbolMetadata()
        {
            var context = new RpcResponseTaskContext<SymbolListResponse>(SingleReponseHandler);
            _session.Ask(RpcMessage.Request(new SymbolListRequest()), context);
            var res = context.TaskSrc.Task.GetAwaiter().GetResult();
            return res.Symbols;
        }

        internal AccountInfo GetAccountInfo()
        {
            var context = new RpcResponseTaskContext<AccountInfoResponse>(SingleReponseHandler);
            _session.Ask(RpcMessage.Request(new AccountInfoRequest()), context);
            var res = context.TaskSrc.Task.GetAwaiter().GetResult();
            return res.Account;
        }

        internal List<OrderInfo> GetOrderList()
        {
            var context = new RpcListResponseTaskContext<OrderInfo>(OrderListReponseHandler);
            _session.Ask(RpcMessage.Request(new OrderListRequest()), context);
            return context.TaskSrc.Task.GetAwaiter().GetResult();
        }

        internal List<PositionInfo> GetPositionList()
        {
            var context = new RpcListResponseTaskContext<PositionInfo>(PositionListReponseHandler);
            _session.Ask(RpcMessage.Request(new PositionListRequest()), context);
            return context.TaskSrc.Task.GetAwaiter().GetResult();
        }

        internal void SendOpenOrder(OpenOrderRequest request)
        {
            var context = new RpcResponseTaskContext<VoidResponse>(SingleReponseHandler);
            _session.Ask(RpcMessage.Request(request), context);
            context.TaskSrc.Task.GetAwaiter().GetResult();
        }

        internal void SendModifyOrder(ModifyOrderRequest request)
        {
            var context = new RpcResponseTaskContext<VoidResponse>(SingleReponseHandler);
            _session.Ask(RpcMessage.Request(request), context);
            context.TaskSrc.Task.GetAwaiter().GetResult();
        }

        internal void SendCloseOrder(CloseOrderRequest request)
        {
            var context = new RpcResponseTaskContext<VoidResponse>(SingleReponseHandler);
            _session.Ask(RpcMessage.Request(request), context);
            context.TaskSrc.Task.GetAwaiter().GetResult();
        }

        internal void SendCancelOrder(CancelOrderRequest request)
        {
            var context = new RpcResponseTaskContext<VoidResponse>(SingleReponseHandler);
            _session.Ask(RpcMessage.Request(request), context);
            context.TaskSrc.Task.GetAwaiter().GetResult();
        }

        internal IAsyncPagedEnumerator<Domain.TradeReportInfo> GetTradeHistory(TradeHistoryRequest request)
        {
            var callId = RpcMessage.GenerateCallId();
            var context = new PagedEnumeratorAdapter<Domain.TradeReportInfo>(TradeHistoryPageResponseHandler,
                () => _session.Tell(RpcMessage.Request(new TradeHistoryRequestNextPage(), callId)),
                () => _session.Tell(RpcMessage.Request(new TradeHistoryRequestDispose(), callId)));
            _session.Ask(RpcMessage.Request(request, callId), context);
            return context;
        }


        public void SetSession(RpcSession session)
        {
            _session = session;
        }

        public void HandleNotification(string callId, Any payload)
        {
            if (payload.Is(Domain.OrderExecReport.Descriptor))
                OrderExecReportNotificationHandler(payload);
            else if (payload.Is(Domain.PositionExecReport.Descriptor))
                PositionExecReportNotificationHandler(payload);
            if (payload.Is(BalanceOperation.Descriptor))
                BalanceOperationNotificationHandler(payload);

        }

        public Any HandleRequest(string callId, Any payload)
        {
            throw new NotImplementedException();
        }


        private bool AttachPluginResponseHandler(TaskCompletionSource<bool> taskSrc, Any payload)
        {
            if (payload.Is(ErrorResponse.Descriptor))
            {
                var error = payload.Unpack<ErrorResponse>();
                taskSrc.TrySetException(new Exception(error.Message));
                return true;
            }
            var response = payload.Unpack<AttachPluginResponse>();
            taskSrc.TrySetResult(response.Success);
            return true;
        }

        private bool OrderListReponseHandler(IObserver<RepeatedField<OrderInfo>> observer, Any payload)
        {
            if (TryGetError(payload, out var ex))
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
            if (TryGetError(payload, out var ex))
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
            if (TryGetError(payload, out var ex))
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


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryGetError(Any payload, out Exception ex)
        {
            ex = null;
            if (payload.Is(ErrorResponse.Descriptor))
            {
                var error = payload.Unpack<ErrorResponse>();
                ex = new Exception(error.Message);
                return true;
            }
            return false;
        }

        private bool SingleReponseHandler<T>(TaskCompletionSource<T> taskSrc, Any payload) where T : IMessage, new()
        {
            if (TryGetError(payload, out var ex))
            {
                taskSrc.TrySetException(ex);
            }
            else
            {
                var response = payload.Unpack<T>();
                taskSrc.TrySetResult(response);
            }

            return true;
        }

        private bool ListReponseHandler<T>(IObserver<RepeatedField<T>> observer, Any payload) where T : IMessage, new()
        {
            if (TryGetError(payload, out var ex))
            {
                observer.OnError(ex);
            }
            else
            {
                var response = payload.Unpack<T>();
                //observer.OnNext(response.Items);
                //if (!response.IsFinal)
                //    return false;

                observer.OnCompleted();
            }

            return true;
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
