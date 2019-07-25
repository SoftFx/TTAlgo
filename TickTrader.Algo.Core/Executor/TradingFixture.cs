﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;
using TickTrader.Algo.Core.Lib;

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

            if (_dataProvider != null)
                _dataProvider.SyncInvoke(Init);
        }

        public void Restart()
        {
            if (_dataProvider != null)
            {
                _dataProvider.SyncInvoke(() =>
                {
                    Deinit();
                    Init();
                });
            }
        }

        private void Init()
        {
            var builder = context.Builder;

            _account = builder.Account;
            _symbols = builder.Symbols;

            _dataProvider.OrderUpdated += DataProvider_OrderUpdated;
            _dataProvider.BalanceUpdated += DataProvider_BalanceUpdated;
            //_dataProvider.PositionUpdated += DataProvider_PositionUpdated;

            var accType = _dataProvider.AccountInfo.Type;

            currencies = builder.Currencies.CurrencyListImp.ToDictionary(c => c.Name);

            builder.Account.Orders.Clear();
            builder.Account.NetPositions.Clear();
            builder.Account.Assets.Clear();

            builder.Account.Update(_dataProvider.AccountInfo, currencies);

            foreach (var order in _dataProvider.GetOrders())
                builder.Account.Orders.Add(order, _account);
            foreach (var position in _dataProvider.GetPositions())
                builder.Account.NetPositions.UpdatePosition(position.PositionInfo);
        }

        public void Stop()
        {
            _dataProvider.SyncInvoke(Deinit);
        }

        private void Deinit()
        {
            _dataProvider.OrderUpdated -= DataProvider_OrderUpdated;
            _dataProvider.BalanceUpdated -= DataProvider_BalanceUpdated;
            //_dataProvider.PositionUpdated -= DataProvider_PositionUpdated;
        }

        private bool CallListener(OrderExecReport eReport)
        {
            if (eReport.OperationId != null)
                return InvokeListener(eReport.OperationId, eReport);
            return false;
        }

        private bool InvokeListener(string operationId, OrderExecReport rep)
        {
            Action<OrderExecReport> listener;
            if (reportListeners.TryGetValue(operationId, out listener))
            {
                listener(rep);
                return true;
            }
            return false;
        }

        private OrderAccessor ApplyOrderEntity(OrderExecReport eReport, OrdersCollection collection)
        {
            var accProxy = context.Builder.Account;

            if (eReport.OrderCopy.Type == OrderType.Market && accProxy.Type == AccountTypes.Gross) // workaround for Gross accounts
            {
                eReport.OrderCopy.Type = OrderType.Position;
                eReport.Action = OrderEntityAction.Updated;
            }

            if (eReport.Action == OrderEntityAction.Added)
                return collection.Add(eReport.OrderCopy, _account);
            if (eReport.Action == OrderEntityAction.Removed)
                return collection.UpdateAndRemove(eReport.OrderCopy);
            if (eReport.Action == OrderEntityAction.Updated)
                return collection.Replace(eReport.OrderCopy);

            return new OrderAccessor(eReport.OrderCopy, _symbols.GetOrDefault, accProxy.Leverage);
        }

        private void DataProvider_BalanceUpdated(BalanceOperationReport report)
        {
            context.EnqueueTradeUpdate(b =>
            {
                var accProxy = context.Builder.Account;

                if (accProxy.Type == Api.AccountTypes.Gross || accProxy.Type == Api.AccountTypes.Net)
                {
                    accProxy.Balance = (decimal)report.Balance;
                    context.Logger.NotifyDespositWithdrawal(report.Amount, (CurrencyEntity)accProxy.BalanceCurrencyInfo);
                    context.EnqueueEvent(builder => accProxy.FireBalanceUpdateEvent());
                }
                else if (accProxy.Type == Api.AccountTypes.Cash)
                {
                    AssetChangeType assetChange;
                    var asset = accProxy.Assets.Update(new AssetEntity(report.Balance, report.CurrencyCode), currencies, out assetChange);
                    var currencyInfo = currencies.GetOrStub(report.CurrencyCode);
                    if (assetChange != AssetChangeType.NoChanges)
                    {
                        context.Logger.NotifyDespositWithdrawal(report.Amount, (CurrencyEntity)currencyInfo);
                        context.EnqueueEvent(builder => accProxy.Assets.FireModified(new AssetUpdateEventArgsImpl(asset)));
                        context.EnqueueEvent(builder => accProxy.FireBalanceUpdateEvent());
                    }
                }
            });
        }

        //private void DataProvider_PositionUpdated(PositionExecReport report)
        //{
        //    context.EnqueueTradeUpdate(b =>
        //    {
        //        var accProxy = context.Builder.Account;
        //        var positions = accProxy.NetPositions;

        //        var oldPos = positions.GetPositionOrNull(report.PositionInfo.Symbol);
        //        var clone = oldPos?.Clone() ?? PositionAccessor.CreateEmpty(report.PositionInfo.Symbol);
        //        var pos = positions.UpdatePosition(report);
        //        var isClosed = report.ExecAction == OrderExecAction.Closed;

        //        context.EnqueueEvent(builder => positions.FirePositionUpdated(new PositionModifiedEventArgsImpl(clone, pos, isClosed)));
        //    });

        //}

        private void UpdatePosition(OrderExecReport report)
        {
            if (report.NetPosition != null)
            {
                var accProxy = context.Builder.Account;
                var positions = accProxy.NetPositions;

                var oldPos = positions.GetPositionOrNull(report.NetPosition.Symbol);
                var clone = oldPos?.Clone() ?? PositionAccessor.CreateEmpty(report.NetPosition.Symbol, _symbols.GetOrDefault, accProxy.Leverage);
                var pos = positions.UpdatePosition(report.NetPosition);
                var isClosed = report.ExecAction == OrderExecAction.Closed;

                context.EnqueueEvent(builder => positions.FirePositionUpdated(new PositionModifiedEventArgsImpl(clone, pos, isClosed)));
            }
        }

        private void DataProvider_OrderUpdated(OrderExecReport eReport)
        {
            context.EnqueueTradeUpdate(b =>
            {
                UpdateOrders(b, eReport);
                UpdateBalance(b, eReport);
            });
        }

        private void UpdateOrders(PluginBuilder builder, OrderExecReport eReport)
        {
            System.Diagnostics.Debug.WriteLine($"ER: {eReport.Action} {(eReport.OrderCopy != null ? $"#{eReport.OrderCopy.Id} {eReport.OrderCopy.Type}" : "no order copy")}");

            UpdatePosition(eReport);

            var orderCollection = builder.Account.Orders;
            if (eReport.ExecAction == OrderExecAction.Activated)
            {
                var order = ApplyOrderEntity(eReport, orderCollection);
                var clone = order.Clone();
                var isOwnOrder = CallListener(eReport);
                if (!isOwnOrder && !IsInvisible(clone))
                    context.Logger.NotifyOrderActivation(clone);
                context.EnqueueEvent(b => orderCollection.FireOrderActivated(new OrderActivatedEventArgsImpl(clone)));
            }
            else if (eReport.ExecAction == OrderExecAction.Opened)
            {
                var order = ApplyOrderEntity(eReport, orderCollection);
                var clone = order.Clone();
                var isOwnOrder = CallListener(eReport);
                if (!isOwnOrder && !IsInvisible(clone))
                    context.Logger.NotifyOrderOpened(clone);
                context.EnqueueEvent(b => b.Account.Orders.FireOrderOpened(new OrderOpenedEventArgsImpl(clone)));
                //if (order.Type == OrderType.Position)
                //    context.EnqueueEvent(b => b.Account.Orders.FireOrderFilled(new OrderFilledEventArgsImpl(clone, clone)));
            }
            else if (eReport.ExecAction == OrderExecAction.Closed)
            {
                var oldOrder = orderCollection.GetOrderOrNull(eReport.OrderId);
                if (oldOrder != null)
                {
                    var order = ApplyOrderEntity(eReport, orderCollection);
                    var clone = order.Clone();
                    var isOwnOrder = CallListener(eReport);
                    if (!isOwnOrder && !IsInvisible(clone))
                        context.Logger.NotifyOrderClosed(clone);
                    context.EnqueueEvent(b => b.Account.Orders.FireOrderClosed(new OrderClosedEventArgsImpl(clone)));
                }
            }
            else if (eReport.ExecAction == OrderExecAction.Canceled)
            {
                // Limit Ioc doesn't appear in order collection
                var order = ApplyOrderEntity(eReport, orderCollection);
                var clone = order.Clone();
                var isOwnOrder = CallListener(eReport);
                if (!isOwnOrder && !IsInvisible(clone))
                    context.Logger.NotifyOrderCancelation(clone);
                context.EnqueueEvent(b => orderCollection.FireOrderCanceled(new OrderCanceledEventArgsImpl(clone)));
            }
            else if (eReport.ExecAction == OrderExecAction.Expired)
            {
                var oldOrder = orderCollection.GetOrderOrNull(eReport.OrderId);
                if (oldOrder != null)
                {
                    var order = ApplyOrderEntity(eReport, orderCollection);
                    var clone = order.Clone();
                    var args = new OrderCanceledEventArgsImpl(clone);
                    if (!IsInvisible(clone))
                        context.Logger.NotifyOrderExpiration(clone);
                    context.EnqueueEvent(b => orderCollection.FireOrderExpired(args));
                }
            }
            else if (eReport.ExecAction == OrderExecAction.Modified)
            {
                var oldOrder = orderCollection.GetOrderOrNull(eReport.OrderId)?.Clone();
                if (oldOrder != null && eReport.OrderCopy != null)
                {
                    var order = ApplyOrderEntity(eReport, orderCollection);
                    var newOrder = order.Clone();
                    var isOwnOrder = CallListener(eReport);
                    if (!isOwnOrder && !IsInvisible(newOrder))
                        context.Logger.NotifyOrderModification(newOrder);
                    context.EnqueueEvent(b => orderCollection.FireOrderModified(new OrderModifiedEventArgsImpl(oldOrder, newOrder)));
                }
            }
            else if (eReport.ExecAction == OrderExecAction.Filled)
            {
                if (eReport.OrderCopy.Type == OrderType.Market)
                {
                    // market orders are never added to orders collection. Cash account has actually limit IoC
                    var clone = new OrderAccessor(eReport.OrderCopy, _symbols.GetOrDefault, _account.Leverage);
                    if (clone != null)
                    {
                        var isOwnOrder = CallListener(eReport);
                        if (!isOwnOrder && !IsInvisible(clone))
                            context.Logger.NotifyOrderFill(clone);
                        context.EnqueueEvent(b => b.Account.Orders.FireOrderOpened(new OrderOpenedEventArgsImpl(clone)));
                        context.EnqueueEvent(b => orderCollection.FireOrderFilled(new OrderFilledEventArgsImpl(clone, clone)));
                    }
                }
                else
                {
                    // pending orders
                    var oldOrder = orderCollection.GetOrderOrNull(eReport.OrderId)?.Clone();
                    if (oldOrder != null && eReport.OrderCopy != null)
                    {
                        var order = ApplyOrderEntity(eReport, orderCollection);
                        var clone = order.Clone();
                        if (!IsInvisible(clone))
                            context.Logger.NotifyOrderFill(clone);
                        context.EnqueueEvent(b => orderCollection.FireOrderFilled(new OrderFilledEventArgsImpl(oldOrder, clone)));
                    }
                    else
                    {
                        var clone = new OrderAccessor(eReport.OrderCopy, _symbols.GetOrDefault, _account.Leverage);
                        if (clone != null)
                        {
                            if (!IsInvisible(clone))
                                context.Logger.NotifyOrderFill(clone);
                            context.EnqueueEvent(b => orderCollection.FireOrderFilled(new OrderFilledEventArgsImpl(clone, clone)));
                        }
                        CallListener(eReport);
                    }
                }
            }
            else if (eReport.ExecAction == OrderExecAction.Rejected)
            {
                CallListener(eReport);
            }
        }

        private void UpdateBalance(PluginBuilder builder, OrderExecReport eReport)
        {
            if (eReport.ExecAction == OrderExecAction.Rejected)
                return;

            var acc = builder.Account;

            if (acc.Type == Api.AccountTypes.Gross || acc.Type == Api.AccountTypes.Net)
            {
                var newBalance = (decimal?)eReport.NewBalance.Value;

                if (eReport.NewBalance != null && acc.Balance != newBalance.Value)
                {
                    acc.Balance = newBalance.Value;
                    context.EnqueueEvent(b => acc.FireBalanceUpdateEvent());
                }
            }
            else if (acc.Type == Api.AccountTypes.Cash)
            {
                if (eReport.Assets != null)
                {
                    bool hasChanges = false;
                    foreach (var asset in eReport.Assets)
                    {
                        AssetChangeType assetChange;
                        var assetModel = acc.Assets.Update(new AssetEntity(asset.Volume, asset.Currency), currencies, out assetChange);
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

        public Task<TradeResultEntity> OpenOrder(bool isAysnc, OpenOrderRequest request)
        {
            return ExecTradeRequest(isAysnc, request, (r, e, c) => e.SendOpenOrder(c, r));
        }

        public Task<TradeResultEntity> CancelOrder(bool isAysnc, CancelOrderRequest request)
        {
            return ExecTradeRequest(isAysnc, request, (r, e, c) => e.SendCancelOrder(c, r));
        }

        public Task<TradeResultEntity> ModifyOrder(bool isAysnc, ReplaceOrderRequest request)
        {
            return ExecTradeRequest(isAysnc, request, (r, e, c) => e.SendModifyOrder(c, r));
        }

        public Task<TradeResultEntity> CloseOrder(bool isAysnc, CloseOrderRequest request)
        {
            if (request.ByOrderId != null)
                return ExecDoubleOrderTradeRequest(isAysnc, request, (r, e, c) => e.SendCloseOrder(c, r));
            else
                return ExecTradeRequest(isAysnc, request, (r, e, c) => e.SendCloseOrder(c, r));
        }

        #endregion

        private Task<TradeResultEntity> ExecTradeRequest<TRequest>(bool isAsync, TRequest orderRequest,
            Action<TRequest, ITradeExecutor, CrossDomainCallback<OrderCmdResultCodes>> executorInvoke)
            where TRequest : OrderRequest
        {
            var resultTask = new TaskCompletionSource<TradeResultEntity>();
            var callbackTask = new TaskCompletionSource<TradeResultEntity>();

            string operationId = Guid.NewGuid().ToString();

            reportListeners.Add(operationId, rep =>
            {
                reportListeners.Remove(operationId);
                resultTask.TrySetResult(new TradeResultEntity(rep.ResultCode, rep.OrderCopy));
            });

            orderRequest.OperationId = operationId;

            var callback = new CrossDomainCallback<OrderCmdResultCodes>();

            callback.Action = code =>
            {
                if (code != OrderCmdResultCodes.Ok)
                    context.EnqueueTradeUpdate(b => InvokeListener(operationId, new OrderExecReport() { ResultCode = code }));

                callback.Dispose();
            };

            executorInvoke(orderRequest, _executor, callback);

            if (!isAsync)
            {
                while (!resultTask.Task.IsCompleted)
                    context.ProcessNextOrderUpdate();
            }

            return resultTask.Task;
        }

        private async Task<TradeResultEntity> ExecDoubleOrderTradeRequest<TRequest>(bool isAsync, TRequest orderRequest,
            Action<TRequest, ITradeExecutor, CrossDomainCallback<OrderCmdResultCodes>> executorInvoke)
            where TRequest : OrderRequest
        {
            var resultTask = new TaskCompletionSource<TradeResultEntity>();
            var resultContainer = new List<OrderEntity>(2);

            string operationId = Guid.NewGuid().ToString();

            reportListeners.Add(operationId, rep =>
            {
                resultContainer.Add(rep.OrderCopy);
                if (resultContainer.Count == 2)
                {
                    reportListeners.Remove(operationId);
                    resultTask.TrySetResult(new TradeResultEntity(rep.ResultCode, rep.OrderCopy));
                }
            });

            Action<OrderCmdResultCodes> callbackAction = code =>
            {
                if (code != OrderCmdResultCodes.Ok)
                    context.EnqueueTradeUpdate(b => InvokeListener(operationId, new OrderExecReport() { ResultCode = code }));
            };

            orderRequest.OperationId = operationId;

            using (var callback = new CrossDomainCallback<OrderCmdResultCodes>(callbackAction))
            {
                executorInvoke(orderRequest, _executor, callback);

                if (!isAsync)
                {
                    while (!resultTask.Task.IsCompleted)
                        context.ProcessNextOrderUpdate();
                }

                return await resultTask.Task;
            }
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
