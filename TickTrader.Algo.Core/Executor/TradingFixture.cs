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
    internal class TradingFixture : CrossDomainObject, ITradeApi
    {
        private IFixtureContext context;
        private Dictionary<string, Currency> currencies;
        private AccountAccessor _account;
        private SymbolsCollection _symbols;
        private ITradeExecutor _executor;

        private Dictionary<string, Action<OrderExecReport>> reportListeners = new Dictionary<string, Action<OrderExecReport>>();

        public TradingFixture(IFixtureContext context)
        {
            this.context = context;
        }

        public IAccountInfoProvider DataProvider { get; set; }

        internal ITradeExecutor Executor { get { return _executor; } set { _executor = value; } }

        public void Start()
        {
            if (DataProvider != null)
                DataProvider.SyncInvoke(Init);
        }

        public void Restart()
        {
            if (DataProvider != null)
            {
                DataProvider.SyncInvoke(() =>
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

            DataProvider.OrderUpdated += DataProvider_OrderUpdated;
            DataProvider.BalanceUpdated += DataProvider_BalanceUpdated;
            DataProvider.PositionUpdated += DataProvider_PositionUpdated;

            var accType = DataProvider.AccountInfo.Type;

            currencies = builder.Currencies.CurrencyListImp.ToDictionary(c => c.Name);

            builder.Account.Orders.Clear();
            builder.Account.NetPositions.Clear();
            builder.Account.Assets.Clear();

            builder.Account.Update(DataProvider.AccountInfo, currencies);

            foreach (var order in DataProvider.GetOrders())
                builder.Account.Orders.Add(order);
            foreach (var position in DataProvider.GetPositions())
                builder.Account.NetPositions.UpdatePosition(position);
        }

        public void Stop()
        {
            DataProvider.SyncInvoke(Deinit);
        }

        private void Deinit()
        {
            DataProvider.OrderUpdated -= DataProvider_OrderUpdated;
            DataProvider.BalanceUpdated -= DataProvider_BalanceUpdated;
            DataProvider.PositionUpdated -= DataProvider_PositionUpdated;
        }

        private void CallListener(OrderExecReport eReport)
        {
            if (eReport.OperationId != null)
                InvokeListener(eReport.OperationId, eReport);
        }

        private void InvokeListener(string operationId, OrderExecReport rep)
        {
            Action<OrderExecReport> listener;
            if (reportListeners.TryGetValue(operationId, out listener))
                listener(rep);
        }

        private OrderAccessor ApplyOrderEntity(OrderExecReport eReport, OrdersCollection collection)
        {
            var accProxy = context.Builder.Account;

            if (eReport.OrderCopy.Type == OrderType.Market && accProxy.Type == AccountTypes.Gross) // workaround for Gross accounts
                eReport.OrderCopy.Type = OrderType.Position;

            if (eReport.Action == OrderEntityAction.Added)
                return collection.Add(eReport.OrderCopy);
            if (eReport.Action == OrderEntityAction.Removed)
                return collection.Remove(eReport.OrderCopy);
            if (eReport.Action == OrderEntityAction.Updated)
                return collection.Replace(eReport.OrderCopy);

            return new OrderAccessor(eReport.OrderCopy, _symbols.GetOrDefault);
        }

        private void DataProvider_BalanceUpdated(BalanceOperationReport report)
        {
            context.EnqueueTradeUpdate(b =>
            {
                var accProxy = context.Builder.Account;

                if (accProxy.Type == Api.AccountTypes.Gross || accProxy.Type == Api.AccountTypes.Net)
                {
                    accProxy.Balance = report.Balance;
                    context.EnqueueEvent(builder => accProxy.FireBalanceUpdateEvent());
                }
                else if (accProxy.Type == Api.AccountTypes.Cash)
                {
                    AssetChangeType assetChange;
                    var asset = accProxy.Assets.Update(new AssetEntity(report.Balance, report.CurrencyCode), currencies, out assetChange);
                    if (assetChange != AssetChangeType.NoChanges)
                    {
                        context.EnqueueEvent(builder => accProxy.Assets.FireModified(new AssetUpdateEventArgsImpl(asset)));
                        context.EnqueueEvent(builder => accProxy.FireBalanceUpdateEvent());
                    }
                }
            });
        }

        private void DataProvider_PositionUpdated(PositionExecReport report)
        {
            context.EnqueueTradeUpdate(b =>
            {
                var accProxy = context.Builder.Account;
                var positions = accProxy.NetPositions;

                var oldPos = positions.GetPositionOrNull(report.PositionInfo.Symbol);
                var clone = oldPos?.Clone() ?? PositionAccessor.CreateEmpty(report.PositionInfo.Symbol);
                var pos = positions.UpdatePosition(report);
                var isClosed = report.ExecAction == OrderExecAction.Closed;

                context.EnqueueEvent(builder => positions.FirePositionUpdated(new PositionModifiedEventArgsImpl(clone, pos, isClosed)));
            });

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

            var orderCollection = builder.Account.Orders;
            if (eReport.ExecAction == OrderExecAction.Activated)
            {
                var order = ApplyOrderEntity(eReport, orderCollection);
                var clone = order.Clone();
                CallListener(eReport);
                context.EnqueueEvent(b => orderCollection.FireOrderActivated(new OrderActivatedEventArgsImpl(clone)));
            }
            else if (eReport.ExecAction == OrderExecAction.Opened)
            {
                var order = ApplyOrderEntity(eReport, orderCollection);
                var clone = order.Clone();
                CallListener(eReport);
                context.EnqueueEvent(b => b.Account.Orders.FireOrderOpened(new OrderOpenedEventArgsImpl(clone)));
            }
            else if (eReport.ExecAction == OrderExecAction.Closed)
            {
                var oldOrder = orderCollection.GetOrderOrNull(eReport.OrderId);
                if (oldOrder != null)
                {
                    var order = ApplyOrderEntity(eReport, orderCollection);
                    var clone = order.Clone();
                    CallListener(eReport);
                    context.EnqueueEvent(b => b.Account.Orders.FireOrderClosed(new OrderClosedEventArgsImpl(clone)));
                }
            }
            else if (eReport.ExecAction == OrderExecAction.Canceled)
            {
                // Limit Ioc doesn't appear in order collection
                var order = ApplyOrderEntity(eReport, orderCollection);
                var clone = order.Clone();
                CallListener(eReport);
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
                    CallListener(eReport);
                    context.EnqueueEvent(b => orderCollection.FireOrderModified(new OrderModifiedEventArgsImpl(oldOrder, newOrder)));
                }
            }
            else if (eReport.ExecAction == OrderExecAction.Filled)
            {
                if (eReport.OrderCopy.Type == OrderType.Market)
                {
                    // market orders are never added to orders collection. Cash account has actually limit IoC
                    var clone = new OrderAccessor(eReport.OrderCopy, _symbols.GetOrDefault);
                    if (clone != null)
                    {
                        CallListener(eReport);
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
                        context.EnqueueEvent(b => orderCollection.FireOrderFilled(new OrderFilledEventArgsImpl(oldOrder, clone)));
                    }
                    else
                    {
                        var clone = new OrderAccessor(eReport.OrderCopy, _symbols.GetOrDefault);
                        if (clone != null)
                        {
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
                if (eReport.NewBalance != null && acc.Balance != eReport.NewBalance.Value)
                {
                    acc.Balance = eReport.NewBalance.Value;
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

        private Task<TradeResultEntity> CreateResult(OrderCmdResultCodes code)
        {
            return Task.FromResult<TradeResultEntity>(new TradeResultEntity(code));
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
