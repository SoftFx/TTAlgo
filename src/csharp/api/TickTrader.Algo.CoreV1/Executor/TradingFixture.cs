using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal class TradingFixture : ITradeApi, IExecutorFixture
    {
        private IFixtureContext context;
        private Dictionary<string, CurrencyInfo> currencies;
        private AccountAccessor _account;
        private SymbolsCollection _symbols;
        private ITradeExecutor _executor;
        private IAccountInfoProvider _dataProvider;
        private bool _isInited;
        private bool _isReinit;
        private bool _connected;

        private Dictionary<string, Action<OrderExecReport>> reportListeners = new Dictionary<string, Action<OrderExecReport>>();

        public TradingFixture(IFixtureContext context)
        {
            this.context = context;
        }

        public void Start()
        {
            _executor = context.TradeExecutor;
            _dataProvider = context.AccInfoProvider;

            context.Builder.TradeApi = this;
            context.Builder.Account.TradeInfoRequested += LazyInit;
        }

        public void Dispose() { }

        public void LazyInit()
        {
            if (!_isInited && _dataProvider != null)
            {
                LazyInitIniternal();
            }
        }

        public void PreRestart()
        {
            if (_dataProvider != null)
            {
                _isReinit = _isInited;
            }
        }

        public void PostRestart()
        {
            if (_isReinit)
            {
                _isReinit = false;
                LazyInitIniternal();
            }
        }

        private void LazyInitIniternal()
        {
            _dataProvider.SyncInvoke(Init);
            context.MarketData.StartCalculators();

            // makes all symbols in symbol list have correct rates
            // also required for account calculator
            // actor synchronization is broken, workaround for this case
            context.FeedStrategy.SubscribeAll();
        }

        private void Init()
        {
            if (_isInited)
                return;

            _isInited = true;

            var builder = context.Builder;

            _account = builder.Account;
            _symbols = builder.Symbols;

            _dataProvider.OrderUpdated += DataProvider_OrderUpdated;
            _dataProvider.BalanceUpdated += DataProvider_BalanceUpdated;
            _dataProvider.PositionUpdated += DataProvider_PositionUpdated;

            currencies = builder.Currencies.Values.Select(u => u.Info).ToDictionary(c => c.Name);
            builder.Account.Init(_dataProvider, currencies);

            _connected = true;
        }

        public void Stop()
        {
            if (_dataProvider != null && _isInited)
            {
                _dataProvider.SyncInvoke(Deinit);
            }
        }

        private void Deinit()
        {
            if (!_isInited)
                return;

            _isInited = false;

            _connected = false;

            _dataProvider.OrderUpdated -= DataProvider_OrderUpdated;
            _dataProvider.BalanceUpdated -= DataProvider_BalanceUpdated;
            _dataProvider.PositionUpdated -= DataProvider_PositionUpdated;

            context.Builder.Account.Deinit();

            CleanupListeners();
        }

        public void ConnectionLost()
        {
            _connected = false;

            CleanupListeners();
        }

        private void CleanupListeners()
        {
            try
            {
                // clean up remaining requests
                foreach (var listener in reportListeners.Values.ToArray())
                {
                    listener.Invoke(new OrderExecReport { ResultCode = OrderExecReport.Types.CmdResultCode.ConnectionError });
                }
                reportListeners.Clear();
            }
            catch (Exception ex)
            {
                context?.OnInternalException(ex);
            }
        }

        private bool CallListener(OrderExecReport eReport)
        {
            if (eReport.OperationId != null)
                return InvokeListener(eReport.OperationId, eReport);
            return false;
        }

        private bool InvokeListener(string operationId, OrderExecReport rep)
        {
            if (reportListeners.TryGetValue(operationId, out Action<OrderExecReport> listener))
            {
                listener(rep);
                return true;
            }
            return false;
        }

        private OrderAccessor ApplyOrderEntity(OrderExecReport eReport, OrdersCollection collection)
        {
            var accProxy = context.Builder.Account;
            var orderType = eReport.OrderCopy.Type;

            var instantOrder = orderType == OrderInfo.Types.Type.Market;

            if (instantOrder && accProxy.Type == AccountInfo.Types.Type.Gross) // workaround for Gross accounts
            {
                eReport.OrderCopy.Type = OrderInfo.Types.Type.Position;
                if (eReport.ExecAction != OrderExecReport.Types.ExecAction.Canceled)
                    eReport.EntityAction = OrderExecReport.Types.EntityAction.Added;
            }

            if (eReport.EntityAction == OrderExecReport.Types.EntityAction.Added)
                return collection.Add(eReport.OrderCopy);
            if (eReport.EntityAction == OrderExecReport.Types.EntityAction.Removed)
                return collection.Remove(eReport.OrderCopy, true);
            if (eReport.EntityAction == OrderExecReport.Types.EntityAction.Updated)
                return collection.Update(eReport.OrderCopy);

            return new OrderAccessor(_symbols.GetOrNull(eReport.OrderCopy.Symbol).Info, eReport.OrderCopy);
        }

        private void DataProvider_BalanceUpdated(BalanceOperation report)
        {
            context.EnqueueTradeUpdate((Action<PluginBuilder>)(b =>
            {
                var accProxy = context.Builder.Account;

                if (accProxy.Type == AccountInfo.Types.Type.Gross || accProxy.Type == AccountInfo.Types.Type.Net)
                {
                    accProxy.Balance = report.Balance;
                    var currencyInfo = currencies.GetOrDefault(report.Currency);

                    if (report.Type == BalanceOperation.Types.Type.DepositWithdrawal)
                    {
                        context.Logger.NotifyBalanceEvent(report.TransactionAmount, accProxy.BalanceCurrencyInfo, report.TransactionAmount > 0 ? BalanceAction.Deposit : BalanceAction.Withdrawal);
                        context.EnqueueEvent(builder => accProxy.FireBalanceUpdateEvent());
                    }

                    if (report.Type == BalanceOperation.Types.Type.Dividend)
                    {
                        context.Logger.NotifyBalanceEvent(report.TransactionAmount, currencyInfo, BalanceAction.Dividend);
                        context.EnqueueEvent(builder => accProxy.FireBalanceDividendEvent(new BalanceDividendEventArgsImpl(report)));
                    }
                }
                else if (accProxy.Type == AccountInfo.Types.Type.Cash)
                {
                    AssetChangeType assetChange;
                    var asset = accProxy.Assets.Update(new AssetInfo(report.Balance, report.Currency), out assetChange);
                    var currencyInfo = currencies.GetOrDefault(report.Currency);
                    if (assetChange != AssetChangeType.NoChanges)
                    {
                        if (report.Type == BalanceOperation.Types.Type.DepositWithdrawal)
                        {
                            context.Logger.NotifyBalanceEvent(report.TransactionAmount, currencyInfo, report.TransactionAmount > 0 ? BalanceAction.Deposit : BalanceAction.Withdrawal);
                            context.EnqueueEvent(builder => accProxy.Assets.FireModified(new AssetUpdateEventArgsImpl(asset)));
                            context.EnqueueEvent(builder => accProxy.FireBalanceUpdateEvent());
                        }
                    }
                }
            }));
        }

        private void DataProvider_PositionUpdated(PositionExecReport report)
        {
            context.EnqueueTradeUpdate(b => UpdatePosition(b, report.PositionCopy, report.ExecAction));
        }

        private void DataProvider_OrderUpdated(OrderExecReport eReport)
        {
            context.EnqueueTradeUpdate(b =>
            {
                UpdateOrders(b, eReport);
                UpdateBalance(b, eReport);
            });
        }

        private void UpdatePosition(PluginBuilder builder, PositionInfo position, OrderExecReport.Types.ExecAction action)
        {
            var positions = builder.Account.NetPositions;

            var oldPos = positions.GetOrNull(position.Symbol);
            var clone = oldPos?.Clone() ?? new PositionAccessor(position.Symbol, _symbols.GetOrNull(position.Symbol));
            var pos = positions.UpdatePosition(position);
            var isClosed = action == OrderExecReport.Types.ExecAction.Closed || pos.Info.IsEmpty;

            if (action == OrderExecReport.Types.ExecAction.Splitted)
            {
                context.EnqueueEvent(b => positions.FirePositionSplitted(new PositionSplittedEventArgsImpl(clone, pos, isClosed)));
                context.Logger.NotifyPositionSplitting(pos);
            }
            else
                context.EnqueueEvent(b => positions.FirePositionUpdated(new PositionModifiedEventArgsImpl(clone, pos, isClosed)));
        }

        private void UpdateOrders(PluginBuilder builder, OrderExecReport eReport)
        {
            System.Diagnostics.Debug.WriteLine($"ER: {eReport.EntityAction} {(eReport.OrderCopy != null ? $"#{eReport.OrderCopy.Id} {eReport.OrderCopy.Type}" : "no order copy")}");

            if (eReport.NetPositionCopy != null)
                UpdatePosition(builder, eReport.NetPositionCopy, eReport.ExecAction); // applied position bounded to order fill

            var orderCollection = builder.Account.Orders;
            if (eReport.ExecAction == OrderExecReport.Types.ExecAction.Activated)
            {
                var order = ApplyOrderEntity(eReport, orderCollection);
                var clone = order.Clone();
                var isOwnOrder = CallListener(eReport);
                if (!isOwnOrder && !IsInvisible(clone))
                    context.Logger.NotifyOrderEvent(clone.Info, eReport.ExecAction);
                context.EnqueueEvent(b => orderCollection.FireOrderActivated(new OrderActivatedEventArgsImpl(clone)));
            }
            else if (eReport.ExecAction == OrderExecReport.Types.ExecAction.Opened)
            {
                var order = ApplyOrderEntity(eReport, orderCollection);
                var clone = order.Clone();
                var isOwnOrder = CallListener(eReport);
                if (!isOwnOrder && !IsInvisible(clone))
                    context.Logger.NotifyOrderEvent(clone.Info, eReport.ExecAction);
                context.EnqueueEvent(b => b.Account.Orders.FireOrderOpened(new OrderOpenedEventArgsImpl(clone)));
            }
            else if (eReport.ExecAction == OrderExecReport.Types.ExecAction.Closed)
            {
                var oldOrder = orderCollection.GetOrNull(eReport.OrderCopy.Id);
                if (oldOrder != null)
                {
                    var order = ApplyOrderEntity(eReport, orderCollection);
                    var clone = order.Clone();
                    var isOwnOrder = CallListener(eReport);
                    if (!isOwnOrder && !IsInvisible(clone))
                        context.Logger.NotifyOrderEvent(clone.Info, eReport.ExecAction);
                    context.EnqueueEvent(b => b.Account.Orders.FireOrderClosed(new OrderClosedEventArgsImpl(clone)));
                }
            }
            else if (eReport.ExecAction == OrderExecReport.Types.ExecAction.Canceled)
            {
                // Limit Ioc doesn't appear in order collection
                var order = ApplyOrderEntity(eReport, orderCollection);
                var clone = order.Clone();
                var isOwnOrder = CallListener(eReport);
                if (!isOwnOrder && !IsInvisible(clone))
                    context.Logger.NotifyOrderEvent(clone.Info, eReport.ExecAction);
                context.EnqueueEvent(b => orderCollection.FireOrderCanceled(new OrderCanceledEventArgsImpl(clone)));
            }
            else if (eReport.ExecAction == OrderExecReport.Types.ExecAction.Expired)
            {
                var oldOrder = orderCollection.GetOrNull(eReport.OrderCopy.Id);
                if (oldOrder != null)
                {
                    var order = ApplyOrderEntity(eReport, orderCollection);
                    var clone = order.Clone();
                    if (!IsInvisible(clone))
                        context.Logger.NotifyOrderEvent(clone.Info, eReport.ExecAction);
                    context.EnqueueEvent(b => orderCollection.FireOrderExpired(new OrderExpiredEventArgsImpl(clone)));
                }
            }
            else if (eReport.ExecAction == OrderExecReport.Types.ExecAction.Modified)
            {
                var oldOrder = orderCollection.GetOrNull(eReport.OrderCopy.Id)?.Clone();
                if (oldOrder != null && eReport.OrderCopy != null)
                {
                    var order = ApplyOrderEntity(eReport, orderCollection);
                    var newOrder = order.Clone();
                    var isOwnOrder = CallListener(eReport);
                    if (!isOwnOrder && !IsInvisible(newOrder))
                        context.Logger.NotifyOrderEvent(newOrder.Info, eReport.ExecAction);

                    context.EnqueueEvent(b => orderCollection.FireOrderModified(new OrderModifiedEventArgsImpl(oldOrder, newOrder)));
                }
            }
            else if (eReport.ExecAction == OrderExecReport.Types.ExecAction.Splitted)
            {
                var oldOrder = orderCollection.GetOrNull(eReport.OrderCopy.Id)?.Clone();
                if (oldOrder != null && eReport.OrderCopy != null)
                {
                    var order = ApplyOrderEntity(eReport, orderCollection);
                    var newOrder = order.Clone();
                    var isOwnOrder = CallListener(eReport);
                    if (!isOwnOrder && !IsInvisible(newOrder))
                        context.Logger.NotifyOrderEvent(newOrder.Info, eReport.ExecAction);

                    context.EnqueueEvent(b => orderCollection.FireOrderSplitted(new OrderSplittedEventArgsImpl(oldOrder, newOrder)));
                }
            }
            else if (eReport.ExecAction == OrderExecReport.Types.ExecAction.Filled)
            {
                if (eReport.OrderCopy.Type == OrderInfo.Types.Type.Market)
                {
                    // market orders are never added to orders collection. Cash account has actually limit IoC
                    var clone = new OrderAccessor(_symbols.GetOrNull(eReport.OrderCopy.Symbol).Info, eReport.OrderCopy);
                    if (clone != null)
                    {
                        var isOwnOrder = CallListener(eReport);
                        if (!isOwnOrder && !IsInvisible(clone))
                            context.Logger.NotifyOrderEvent(clone.Info, eReport.ExecAction);
                        context.EnqueueEvent(b => b.Account.Orders.FireOrderOpened(new OrderOpenedEventArgsImpl(clone)));
                        context.EnqueueEvent(b => orderCollection.FireOrderFilled(new OrderFilledEventArgsImpl(clone, clone)));
                    }
                }
                else
                {
                    // pending orders
                    var oldOrder = orderCollection.GetOrNull(eReport.OrderCopy.Id)?.Clone();
                    if (oldOrder != null && eReport.OrderCopy != null)
                    {
                        var order = ApplyOrderEntity(eReport, orderCollection);
                        var clone = order.Clone();
                        if (!IsInvisible(clone))
                            context.Logger.NotifyOrderEvent(clone.Info, eReport.ExecAction);
                        context.EnqueueEvent(b => orderCollection.FireOrderFilled(new OrderFilledEventArgsImpl(oldOrder, clone)));
                    }
                    else
                    {
                        var clone = new OrderAccessor(_symbols.GetOrNull(eReport.OrderCopy.Symbol).Info, eReport.OrderCopy);
                        if (clone != null)
                        {
                            if (!IsInvisible(clone))
                                context.Logger.NotifyOrderEvent(clone.Info, eReport.ExecAction);
                            context.EnqueueEvent(b => orderCollection.FireOrderFilled(new OrderFilledEventArgsImpl(clone, clone)));
                        }
                        CallListener(eReport);
                    }
                }
            }
            else if (eReport.ExecAction == OrderExecReport.Types.ExecAction.Rejected)
            {
                CallListener(eReport);
            }
        }

        private void UpdateBalance(PluginBuilder builder, OrderExecReport eReport)
        {
            if (eReport.ExecAction == OrderExecReport.Types.ExecAction.Rejected)
                return;

            var acc = builder.Account;

            if (acc.Type == AccountInfo.Types.Type.Gross || acc.Type == AccountInfo.Types.Type.Net)
            {
                var newBalance = eReport.NewBalance;

                if (eReport.NewBalance != null && acc.Balance != newBalance.Value)
                {
                    acc.Balance = newBalance.Value;
                    context.EnqueueEvent(b => acc.FireBalanceUpdateEvent());
                }
            }
            else if (acc.Type == AccountInfo.Types.Type.Cash)
            {
                if (eReport.Assets != null)
                {
                    bool hasChanges = false;
                    foreach (var asset in eReport.Assets)
                    {
                        AssetChangeType assetChange;
                        var assetModel = acc.Assets.Update(asset, out assetChange);
                        if (assetChange != AssetChangeType.NoChanges)
                        {
                            hasChanges = true;
                            var args = new AssetUpdateEventArgsImpl(assetModel);
                            context.EnqueueEvent(b => acc.Assets.FireModified(args));
                        }
                    }
                    if (hasChanges)
                        context.EnqueueEvent(b => acc.FireBalanceUpdateEvent());
                }
            }
        }

        private bool IsInvisible(OrderAccessor order)
        {
            return context.Builder.Isolated && order.Info.InstanceId != context.Builder.InstanceId;
        }

        #region TradeCommands impl

        public Task<TradeResultInfo> OpenOrder(bool isAsync, Domain.OpenOrderRequest request)
        {
            return ExecTradeRequest(isAsync, request, (r, e) => e.SendOpenOrder(r));
        }

        public Task<TradeResultInfo> CancelOrder(bool isAsync, CancelOrderRequest request)
        {
            return ExecTradeRequest(isAsync, request, (r, e) => e.SendCancelOrder(r));
        }

        public Task<TradeResultInfo> ModifyOrder(bool isAsync, Domain.ModifyOrderRequest request)
        {
            return ExecTradeRequest(isAsync, request, (r, e) => e.SendModifyOrder(r));
        }

        public Task<TradeResultInfo> CloseOrder(bool isAsync, Domain.CloseOrderRequest request)
        {
            if (request.ByOrderId != null)
                return ExecDoubleOrderTradeRequest(isAsync, request, (r, e) => e.SendCloseOrder(r));
            else
                return ExecTradeRequest(isAsync, request, (r, e) => e.SendCloseOrder(r));
        }

        #endregion


        private async Task<TradeResultInfo> ExecTradeRequest<TRequest>(bool isAsync, TRequest orderRequest,
            Action<TRequest, ITradeExecutor> executorInvoke)
            where TRequest : ITradeRequest
        {
            if (!_connected)
            {
                if (isAsync)
                    await Task.Yield(); // avoiding synchronous execution

                return new TradeResultInfo(OrderExecReport.Types.CmdResultCode.ConnectionError, null);
            }

            var resultTask = new TaskCompletionSource<TradeResultInfo>();

            string operationId = Guid.NewGuid().ToString();

            reportListeners.Add(operationId, rep =>
            {
                reportListeners.Remove(operationId);
                resultTask.TrySetResult(new TradeResultInfo(rep.ResultCode, rep.OrderCopy));
            });

            orderRequest.OperationId = operationId;

            GenerateSubOperationIds(orderRequest);

            executorInvoke(orderRequest, _executor);

            if (!isAsync)
            {
                while (!resultTask.Task.IsCompleted)
                    context.ProcessNextOrderUpdate();
            }

            return await resultTask.Task;
        }

        private async Task<TradeResultInfo> ExecDoubleOrderTradeRequest<TRequest>(bool isAsync, TRequest orderRequest,
            Action<TRequest, ITradeExecutor> executorInvoke)
            where TRequest : ITradeRequest
        {
            if (!_connected)
            {
                if (isAsync)
                    await Task.Yield(); // avoiding synchronous execution

                return new TradeResultInfo(OrderExecReport.Types.CmdResultCode.ConnectionError, null);
            }

            var resultTask = new TaskCompletionSource<TradeResultInfo>();

            var resultContainer = new List<OrderInfo>(2);

            string operationId = Guid.NewGuid().ToString();

            reportListeners.Add(operationId, rep =>
            {
                resultContainer.Add(rep.OrderCopy);
                if (resultContainer.Count == 2)
                {
                    reportListeners.Remove(operationId);
                    resultTask.TrySetResult(new TradeResultInfo(rep.ResultCode, rep.OrderCopy));
                }
            });

            orderRequest.OperationId = operationId;

            executorInvoke(orderRequest, _executor);

            if (!isAsync)
            {
                while (!resultTask.Task.IsCompleted)
                    context.ProcessNextOrderUpdate();
            }

            return await resultTask.Task;
        }

        private static void GenerateSubOperationIds(ITradeRequest request)
        {
            if (string.IsNullOrEmpty(request.OperationId))
                request.OperationId = Guid.NewGuid().ToString();

            if (request.SubRequests != null)
                foreach (var subRequest in request.SubRequests)
                    GenerateSubOperationIds(subRequest);
        }
    }
}
