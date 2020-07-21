﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    internal class TradingFixture : CrossDomainObject, ITradeApi, IExecutorFixture
    {
        private IFixtureContext context;
        private Dictionary<string, Currency> currencies;
        private AccountAccessor _account;
        private SymbolsCollection _symbols;
        private ITradeExecutor _executor;
        private IAccountInfoProvider _dataProvider;
        private bool _isStarted;

        private Dictionary<string, Action<Domain.OrderExecReport>> reportListeners = new Dictionary<string, Action<Domain.OrderExecReport>>();

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

        public void LazyInit()
        {
            if (!_isStarted && _dataProvider != null)
            {
                _isStarted = true;
                _dataProvider.SyncInvoke(Init);
                // makes all symbols in symbol list have correct rates
                // also required for account calculator
                // actor synchronization is broken, workaround for this case
                context.FeedStrategy.SubscribeAll();
            }
        }

        public void Restart()
        {
            if (_dataProvider != null && _isStarted)
            {
                _dataProvider.SyncInvoke(() =>
                {
                    Deinit();
                    Init();
                });
                // makes all symbols in symbol list have correct rates
                // also required for account calculator
                // actor synchronization is broken, workaround for this case
                context.FeedStrategy.SubscribeAll();
            }
        }

        private void Init()
        {
            var builder = context.Builder;

            _account = builder.Account;
            _symbols = builder.Symbols;

            _dataProvider.OrderUpdated += DataProvider_OrderUpdated;
            _dataProvider.BalanceUpdated += DataProvider_BalanceUpdated;
            _dataProvider.PositionUpdated += DataProvider_PositionUpdated;

            currencies = builder.Currencies.CurrencyListImp.ToDictionary(c => c.Name);

            builder.Account.Init(_dataProvider, currencies);
        }

        public void Stop()
        {
            _dataProvider.SyncInvoke(Deinit);
        }

        private void Deinit()
        {
            _dataProvider.OrderUpdated -= DataProvider_OrderUpdated;
            _dataProvider.BalanceUpdated -= DataProvider_BalanceUpdated;
            _dataProvider.PositionUpdated -= DataProvider_PositionUpdated;
        }

        private bool CallListener(Domain.OrderExecReport eReport)
        {
            if (eReport.OperationId != null)
                return InvokeListener(eReport.OperationId, eReport);
            return false;
        }

        private bool InvokeListener(string operationId, Domain.OrderExecReport rep)
        {
            if (reportListeners.TryGetValue(operationId, out Action<Domain.OrderExecReport> listener))
            {
                listener(rep);
                return true;
            }
            return false;
        }

        private OrderAccessor ApplyOrderEntity(Domain.OrderExecReport eReport, OrdersCollection collection)
        {
            var accProxy = context.Builder.Account;
            var orderType = eReport.OrderCopy.Type;
            var instantOrder = orderType == Domain.OrderInfo.Types.Type.Market;

            if (instantOrder && accProxy.Type == AccountInfo.Types.Type.Gross) // workaround for Gross accounts
            {
                eReport.OrderCopy.Type = Domain.OrderInfo.Types.Type.Position;
                if (eReport.ExecAction != Domain.OrderExecReport.Types.ExecAction.Canceled)
                    eReport.EntityAction = Domain.OrderExecReport.Types.EntityAction.Added;
            }

            if (eReport.EntityAction == Domain.OrderExecReport.Types.EntityAction.Added)
                return collection.Add(eReport.OrderCopy, _account);
            if (eReport.EntityAction == Domain.OrderExecReport.Types.EntityAction.Removed)
                return collection.UpdateAndRemove(eReport.OrderCopy);
            if (eReport.EntityAction == Domain.OrderExecReport.Types.EntityAction.Updated)
                return collection.Replace(eReport.OrderCopy);

            return new OrderAccessor(eReport.OrderCopy, _symbols.GetOrDefault, accProxy.Leverage);
        }

        private void DataProvider_BalanceUpdated(Domain.BalanceOperation report)
        {
            context.EnqueueTradeUpdate(b =>
            {
                var accProxy = context.Builder.Account;

                if (accProxy.Type == AccountInfo.Types.Type.Gross || accProxy.Type == AccountInfo.Types.Type.Net)
                {
                    accProxy.Balance = (decimal)report.Balance;
                    var currencyInfo = currencies.GetOrStub(report.Currency);

                    if (report.Type == BalanceOperation.Types.Type.DepositWithdrawal)
                    {
                        context.Logger.NotifyDespositWithdrawal(report.TransactionAmount, (CurrencyEntity)accProxy.BalanceCurrencyInfo);
                        context.EnqueueEvent(builder => accProxy.FireBalanceUpdateEvent());
                    }

                    if (report.Type == BalanceOperation.Types.Type.Dividend)
                    {
                        context.Logger.NotifyDividend(report.TransactionAmount, currencyInfo.Name, ((CurrencyEntity)currencyInfo).Format);
                        context.EnqueueEvent(builder => accProxy.FireBalanceDividendEvent(new BalanceDividendEventArgsImpl(report)));
                    }
                }
                else if (accProxy.Type == AccountInfo.Types.Type.Cash)
                {
                    AssetChangeType assetChange;
                    var asset = accProxy.Assets.Update(new Domain.AssetInfo(report.Balance, report.Currency), currencies, out assetChange);
                    var currencyInfo = currencies.GetOrStub(report.Currency);
                    if (assetChange != AssetChangeType.NoChanges)
                    {
                        if (report.Type == BalanceOperation.Types.Type.DepositWithdrawal)
                        {
                            context.Logger.NotifyDespositWithdrawal(report.TransactionAmount, (CurrencyEntity)currencyInfo);
                            context.EnqueueEvent(builder => accProxy.Assets.FireModified(new AssetUpdateEventArgsImpl(asset)));
                            context.EnqueueEvent(builder => accProxy.FireBalanceUpdateEvent());
                        }
                    }
                }
            });
        }

        private void DataProvider_PositionUpdated(Domain.PositionExecReport report)
        {
            UpdatePosition(report.PositionCopy, report.ExecAction);
        }

        private void DataProvider_OrderUpdated(Domain.OrderExecReport eReport)
        {
            context.EnqueueTradeUpdate(b =>
            {
                UpdateOrders(b, eReport);
                UpdateBalance(b, eReport);
            });
        }

        private void UpdatePosition(PositionInfo position, Domain.OrderExecReport.Types.ExecAction action)
        {
            var accProxy = context.Builder.Account;
            var positions = accProxy.NetPositions;

            var oldPos = positions.GetPositionOrNull(position.Symbol);
            var clone = oldPos?.Clone() ?? PositionAccessor.CreateEmpty(position.Symbol, _symbols.GetOrDefault, accProxy.Leverage);
            var pos = positions.UpdatePosition(position);
            var isClosed = action == Domain.OrderExecReport.Types.ExecAction.Closed || pos.IsEmpty;

            if (action == Domain.OrderExecReport.Types.ExecAction.Splitted)
            {
                context.EnqueueEvent(builder => positions.FirePositionSplitted(new PositionSplittedEventArgsImpl(clone, pos, isClosed)));
                context.Logger.NotifyPositionSplitting(pos);
            }
            else
                context.EnqueueEvent(builder => positions.FirePositionUpdated(new PositionModifiedEventArgsImpl(clone, pos, isClosed)));
        }

        private void UpdateOrders(PluginBuilder builder, Domain.OrderExecReport eReport)
        {
            System.Diagnostics.Debug.WriteLine($"ER: {eReport.EntityAction} {(eReport.OrderCopy != null ? $"#{eReport.OrderCopy.Id} {eReport.OrderCopy.Type}" : "no order copy")}");

            if (eReport.NetPositionCopy != null)
                UpdatePosition(eReport.NetPositionCopy, eReport.ExecAction); // applied position bounded to order fill

            var orderCollection = builder.Account.Orders;
            if (eReport.ExecAction == Domain.OrderExecReport.Types.ExecAction.Activated)
            {
                var order = ApplyOrderEntity(eReport, orderCollection);
                var clone = order.Clone();
                var isOwnOrder = CallListener(eReport);
                if (!isOwnOrder && !IsInvisible(clone))
                    context.Logger.NotifyOrderActivation(clone);
                context.EnqueueEvent(b => orderCollection.FireOrderActivated(new OrderActivatedEventArgsImpl(clone.ApiOrder)));
            }
            else if (eReport.ExecAction == Domain.OrderExecReport.Types.ExecAction.Opened)
            {
                var order = ApplyOrderEntity(eReport, orderCollection);
                var clone = order.Clone();
                var isOwnOrder = CallListener(eReport);
                if (!isOwnOrder && !IsInvisible(clone))
                    context.Logger.NotifyOrderOpened(clone);
                context.EnqueueEvent(b => b.Account.Orders.FireOrderOpened(new OrderOpenedEventArgsImpl(clone.ApiOrder)));
            }
            else if (eReport.ExecAction == Domain.OrderExecReport.Types.ExecAction.Closed)
            {
                var oldOrder = orderCollection.GetOrderOrNull(eReport.OrderCopy.Id);
                if (oldOrder != null)
                {
                    var order = ApplyOrderEntity(eReport, orderCollection);
                    var clone = order.Clone();
                    var isOwnOrder = CallListener(eReport);
                    if (!isOwnOrder && !IsInvisible(clone))
                        context.Logger.NotifyOrderClosed(clone);
                    context.EnqueueEvent(b => b.Account.Orders.FireOrderClosed(new OrderClosedEventArgsImpl(clone.ApiOrder)));
                }
            }
            else if (eReport.ExecAction == Domain.OrderExecReport.Types.ExecAction.Canceled)
            {
                // Limit Ioc doesn't appear in order collection
                var order = ApplyOrderEntity(eReport, orderCollection);
                var clone = order.Clone();
                var isOwnOrder = CallListener(eReport);
                if (!isOwnOrder && !IsInvisible(clone))
                    context.Logger.NotifyOrderCancelation(clone);
                context.EnqueueEvent(b => orderCollection.FireOrderCanceled(new OrderCanceledEventArgsImpl(clone.ApiOrder)));
            }
            else if (eReport.ExecAction == Domain.OrderExecReport.Types.ExecAction.Expired)
            {
                var oldOrder = orderCollection.GetOrderOrNull(eReport.OrderCopy.Id);
                if (oldOrder != null)
                {
                    var order = ApplyOrderEntity(eReport, orderCollection);
                    var clone = order.Clone();
                    if (!IsInvisible(clone))
                        context.Logger.NotifyOrderExpiration(clone);
                    context.EnqueueEvent(b => orderCollection.FireOrderExpired(new OrderExpiredEventArgsImpl(clone.ApiOrder)));
                }
            }
            else if (eReport.ExecAction == Domain.OrderExecReport.Types.ExecAction.Modified)
            {
                var oldOrder = orderCollection.GetOrderOrNull(eReport.OrderCopy.Id)?.Clone();
                if (oldOrder != null && eReport.OrderCopy != null)
                {
                    var order = ApplyOrderEntity(eReport, orderCollection);
                    var newOrder = order.Clone();
                    var isOwnOrder = CallListener(eReport);
                    if (!isOwnOrder && !IsInvisible(newOrder))
                        context.Logger.NotifyOrderModification(newOrder);

                    context.EnqueueEvent(b => orderCollection.FireOrderModified(new OrderModifiedEventArgsImpl(oldOrder.ApiOrder, newOrder.ApiOrder)));
                }
            }
            else if (eReport.ExecAction == Domain.OrderExecReport.Types.ExecAction.Splitted)
            {
                var oldOrder = orderCollection.GetOrderOrNull(eReport.OrderCopy.Id)?.Clone();
                if (oldOrder != null && eReport.OrderCopy != null)
                {
                    var order = ApplyOrderEntity(eReport, orderCollection);
                    var newOrder = order.Clone();
                    var isOwnOrder = CallListener(eReport);
                    if (!isOwnOrder && !IsInvisible(newOrder))
                        context.Logger.NotifyOrderSplitting(newOrder);

                    context.EnqueueEvent(b => orderCollection.FireOrderSplitted(new OrderSplittedEventArgsImpl(oldOrder.ApiOrder, newOrder.ApiOrder)));
                }
            }
            else if (eReport.ExecAction == Domain.OrderExecReport.Types.ExecAction.Filled)
            {
                if (eReport.OrderCopy.Type == Domain.OrderInfo.Types.Type.Market)
                {
                    // market orders are never added to orders collection. Cash account has actually limit IoC
                    var clone = new OrderAccessor(eReport.OrderCopy, _symbols.GetOrDefault, _account.Leverage);
                    if (clone != null)
                    {
                        var isOwnOrder = CallListener(eReport);
                        if (!isOwnOrder && !IsInvisible(clone))
                            context.Logger.NotifyOrderFill(clone);
                        context.EnqueueEvent(b => b.Account.Orders.FireOrderOpened(new OrderOpenedEventArgsImpl(clone.ApiOrder)));
                        context.EnqueueEvent(b => orderCollection.FireOrderFilled(new OrderFilledEventArgsImpl(clone.ApiOrder, clone.ApiOrder)));
                    }
                }
                else
                {
                    // pending orders
                    var oldOrder = orderCollection.GetOrderOrNull(eReport.OrderCopy.Id)?.Clone();
                    if (oldOrder != null && eReport.OrderCopy != null)
                    {
                        var order = ApplyOrderEntity(eReport, orderCollection);
                        var clone = order.Clone();
                        if (!IsInvisible(clone))
                            context.Logger.NotifyOrderFill(clone);
                        context.EnqueueEvent(b => orderCollection.FireOrderFilled(new OrderFilledEventArgsImpl(oldOrder.ApiOrder, clone.ApiOrder)));
                    }
                    else
                    {
                        var clone = new OrderAccessor(eReport.OrderCopy, _symbols.GetOrDefault, _account.Leverage);
                        if (clone != null)
                        {
                            if (!IsInvisible(clone))
                                context.Logger.NotifyOrderFill(clone);
                            context.EnqueueEvent(b => orderCollection.FireOrderFilled(new OrderFilledEventArgsImpl(clone.ApiOrder, clone.ApiOrder)));
                        }
                        CallListener(eReport);
                    }
                }
            }
            else if (eReport.ExecAction == Domain.OrderExecReport.Types.ExecAction.Rejected)
            {
                CallListener(eReport);
            }
        }

        private void UpdateBalance(PluginBuilder builder, Domain.OrderExecReport eReport)
        {
            if (eReport.ExecAction == Domain.OrderExecReport.Types.ExecAction.Rejected)
                return;

            var acc = builder.Account;

            if (acc.Type == AccountInfo.Types.Type.Gross || acc.Type == AccountInfo.Types.Type.Net)
            {
                var newBalance = (decimal?)eReport.NewBalance;

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
                        var assetModel = acc.Assets.Update(asset, currencies, out assetChange);
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
            return context.Builder.Isolated && order.InstanceId != context.Builder.InstanceId;
        }

        #region TradeCommands impl

        public Task<Domain.TradeResultInfo> OpenOrder(bool isAysnc, Domain.OpenOrderRequest request)
        {
            return ExecTradeRequest(isAysnc, request, (r, e) => e.SendOpenOrder(r));
        }

        public Task<Domain.TradeResultInfo> CancelOrder(bool isAysnc, Domain.CancelOrderRequest request)
        {
            return ExecTradeRequest(isAysnc, request, (r, e) => e.SendCancelOrder(r));
        }

        public Task<Domain.TradeResultInfo> ModifyOrder(bool isAysnc, Domain.ModifyOrderRequest request)
        {
            return ExecTradeRequest(isAysnc, request, (r, e) => e.SendModifyOrder(r));
        }

        public Task<Domain.TradeResultInfo> CloseOrder(bool isAysnc, Domain.CloseOrderRequest request)
        {
            if (request.ByOrderId != null)
                return ExecDoubleOrderTradeRequest(isAysnc, request, (r, e) => e.SendCloseOrder(r));
            else
                return ExecTradeRequest(isAysnc, request, (r, e) => e.SendCloseOrder(r));
        }

        #endregion

        private Task<Domain.TradeResultInfo> ExecTradeRequest<TRequest>(bool isAsync, TRequest orderRequest,
            Action<TRequest, ITradeExecutor> executorInvoke)
            where TRequest : ITradeRequest
        {
            var resultTask = new TaskCompletionSource<Domain.TradeResultInfo>();

            string operationId = Guid.NewGuid().ToString();

            reportListeners.Add(operationId, rep =>
            {
                reportListeners.Remove(operationId);
                resultTask.TrySetResult(new Domain.TradeResultInfo(rep.ResultCode, rep.OrderCopy));
            });

            orderRequest.OperationId = operationId;

            executorInvoke(orderRequest, _executor);

            if (!isAsync)
            {
                while (!resultTask.Task.IsCompleted)
                    context.ProcessNextOrderUpdate();
            }

            return resultTask.Task;
        }

        private async Task<Domain.TradeResultInfo> ExecDoubleOrderTradeRequest<TRequest>(bool isAsync, TRequest orderRequest,
            Action<TRequest, ITradeExecutor> executorInvoke)
            where TRequest : ITradeRequest
        {
            var resultTask = new TaskCompletionSource<Domain.TradeResultInfo>();

            var resultContainer = new List<OrderInfo>(2);

            string operationId = Guid.NewGuid().ToString();

            reportListeners.Add(operationId, rep =>
            {
                resultContainer.Add(rep.OrderCopy);
                if (resultContainer.Count == 2)
                {
                    reportListeners.Remove(operationId);
                    resultTask.TrySetResult(new Domain.TradeResultInfo(rep.ResultCode, rep.OrderCopy));
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

        private double ConvertVolume(double volumeInLots, Symbol smbMetadata)
        {
            return smbMetadata.ContractSize * volumeInLots;
        }

        private double RoundVolume(double volumeInLots, Symbol smbMetadata)
        {
            return volumeInLots.Floor(smbMetadata.TradeVolumeStep);
        }

        private double? RoundVolume(double? volumeInLots, Symbol smbMetadata)
        {
            return volumeInLots.Floor(smbMetadata.TradeVolumeStep);
        }

        private double RoundPrice(double price, Symbol smbMetadata, OrderSide side)
        {
            return side == OrderSide.Buy ? price.Ceil(smbMetadata.Digits) : price.Floor(smbMetadata.Digits);
        }

        private double? RoundPrice(double? price, Symbol smbMetadata, OrderSide side)
        {
            return side == OrderSide.Buy ? price.Ceil(smbMetadata.Digits) : price.Floor(smbMetadata.Digits);
        }
    }
}
