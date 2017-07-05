using System;
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
        private AccountEntity _account;
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

        private void Init()
        {
            var builder = context.Builder;

            _account = builder.Account;
            _symbols = builder.Symbols;

            DataProvider.OrderUpdated += DataProvider_OrderUpdated;
            DataProvider.BalanceUpdated += DataProvider_BalanceUpdated;
            DataProvider.PositionUpdated += DataProvider_PositionUpdated;

            var accType = DataProvider.AccountType;

            builder.Account.Id = DataProvider.Account;
            builder.Account.Type = accType;

            currencies = builder.Currencies.CurrencyListImp.ToDictionary(c => c.Name);

            if (accType == Api.AccountTypes.Cash)
            {
                builder.Account.Balance = double.NaN;
                builder.Account.BalanceCurrency = "";
            }
            else
            {
                builder.Account.Balance = (double)DataProvider.Balance;
                builder.Account.BalanceCurrency = DataProvider.BalanceCurrency;
                builder.Account.Leverage = DataProvider.Leverage;
            }

            foreach (var order in DataProvider.GetOrders())
                builder.Account.Orders.Add(order);
            foreach (var position in DataProvider.GetPositions())
                builder.Account.NetPositions.UpdatePosition(position);
            foreach (var asset in DataProvider.GetAssets())
                builder.Account.Assets.Update(asset, currencies);
        }

        public void Stop()
        {
            DataProvider.SyncInvoke(Deinit);
        }

        private void Deinit()
        {
            DataProvider.OrderUpdated -= DataProvider_OrderUpdated;
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

        private static OrderAccessor ApplyOrderEntity(OrderExecReport eReport, OrdersCollection collection)
        {
            if (eReport.Action == OrderEntityAction.Added)
                return collection.Add(eReport.OrderCopy);
            else if (eReport.Action == OrderEntityAction.Removed)
                return collection.Remove(eReport.OrderId);
            else if (eReport.Action == OrderEntityAction.Updated)
                return collection.Replace(eReport.OrderCopy);
            return null;
        }

        private void DataProvider_BalanceUpdated(BalanceOperationReport report)
        {
            context.EnqueueTradeUpdate(b =>
            {
                var accProxy = context.Builder.Account;

                if (accProxy.Type == Api.AccountTypes.Gross || accProxy.Type == Api.AccountTypes.Net)
                {
                    accProxy.Balance = report.Balance;
                    accProxy.FireBalanceUpdateEvent();
                }
                else if (accProxy.Type == Api.AccountTypes.Cash)
                {
                    AssetChangeType assetChange;
                    var asset = accProxy.Assets.Update(new AssetEntity(report.Balance, report.CurrencyCode), currencies, out assetChange);
                    if (assetChange != AssetChangeType.NoChanges)
                        accProxy.Assets.FireModified(new AssetUpdateEventArgsImpl(asset));
                }
            });
        }

        private void DataProvider_PositionUpdated(PositionExecReport report)
        {
            context.EnqueueTradeUpdate(b =>
            {
                var accProxy = context.Builder.Account;
                var positions = accProxy.NetPositions;

                var oldPos = positions.GetPositionOrNull(report.Symbol);
                var clone = oldPos?.Clone() ?? PositionEntity.CreateEmpty(report.Symbol);
                var pos = positions.UpdatePosition(report);
                var isClosed = report.ExecAction == OrderExecAction.Closed;

                positions.FirePositionUpdated(new PositionModifiedEventArgsImpl(clone, pos, isClosed));
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
            var orderCollection = builder.Account.Orders;

            if (eReport.ExecAction == OrderExecAction.Opened)
            {
                var order = ApplyOrderEntity(eReport, orderCollection);
                var clone = order.Clone();
                CallListener(eReport);
                context.EnqueueTradeEvent(b => b.Account.Orders.FireOrderOpened(new OrderOpenedEventArgsImpl(clone)));
            }
            else if (eReport.ExecAction == OrderExecAction.Closed)
            {
                var oldOrder = orderCollection.GetOrderOrNull(eReport.OrderId);
                if (oldOrder != null)
                {
                    var order = ApplyOrderEntity(eReport, orderCollection);
                    var clone = order.Clone();
                    CallListener(eReport);
                    context.EnqueueTradeEvent(b => b.Account.Orders.FireOrderClosed(new OrderClosedEventArgsImpl(clone)));
                }
            }
            else if (eReport.ExecAction == OrderExecAction.Canceled)
            {
                var oldOrder = orderCollection.GetOrderOrNull(eReport.OrderId);
                if (oldOrder != null)
                {
                    var order = ApplyOrderEntity(eReport, orderCollection);
                    var clone = order.Clone();
                    CallListener(eReport);
                    context.EnqueueTradeEvent(b => orderCollection.FireOrderCanceled(new OrderCanceledEventArgsImpl(clone)));
                }
            }
            else if (eReport.ExecAction == OrderExecAction.Expired)
            {
                var oldOrder = orderCollection.GetOrderOrNull(eReport.OrderId);
                if (oldOrder != null)
                {
                    var order = ApplyOrderEntity(eReport, orderCollection);
                    var clone = order.Clone();
                    var args = new OrderCanceledEventArgsImpl(clone);
                    context.EnqueueTradeEvent(b => orderCollection.FireOrderExpired(args));
                }
            }
            else if (eReport.ExecAction == OrderExecAction.Modified)
            {
                var oldOrder = orderCollection.GetOrderOrNull(eReport.OrderId);
                if (oldOrder != null && eReport.OrderCopy != null)
                {
                    var order = ApplyOrderEntity(eReport, orderCollection);
                    var clone = order.Clone();
                    CallListener(eReport);
                    context.EnqueueTradeEvent(b => orderCollection.FireOrderModified(new OrderModifiedEventArgsImpl(oldOrder, clone)));
                }
            }
            else if (eReport.ExecAction == OrderExecAction.Filled)
            {
                if (eReport.OrderCopy.Type == OrderType.Market)
                {
                    // market orders
                    CallListener(eReport);
                    var order = orderCollection.GetOrderOrNull(eReport.OrderId);
                    var clone = order?.Clone() ?? new OrderAccessor(eReport.OrderCopy);
                    context.EnqueueTradeEvent(b => orderCollection.FireOrderFilled(new OrderFilledEventArgsImpl(clone, clone)));
                }
                else
                {
                    // pending orders
                    var oldOrder = orderCollection.GetOrderOrNull(eReport.OrderId);
                    if (oldOrder != null && eReport.OrderCopy != null)
                    {
                        var order = ApplyOrderEntity(eReport, orderCollection);
                        var clone = order.Clone();
                        context.EnqueueTradeEvent(b => orderCollection.FireOrderFilled(new OrderFilledEventArgsImpl(oldOrder, clone)));
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
            var acc = builder.Account;

            if (acc.Type == Api.AccountTypes.Gross || acc.Type == Api.AccountTypes.Net)
            {
                if (eReport.NewBalance != null && acc.Balance != eReport.NewBalance.Value)
                {
                    acc.Balance = eReport.NewBalance.Value;
                    context.EnqueueTradeEvent(b => acc.FireBalanceUpdateEvent());
                }
            }
            else if (acc.Type == Api.AccountTypes.Cash)
            {
                if (eReport.Assets != null)
                {
                    foreach (var asset in eReport.Assets)
                    {
                        var assetModel = acc.Assets.Update(new AssetEntity(asset.Volume, asset.Currency), currencies);
                        var args = new AssetUpdateEventArgsImpl(assetModel);
                        context.EnqueueTradeEvent(b => acc.Assets.FireModified(args));
                    }
                }
            }
        }

        #region TradeCommands impl

        public Task<OrderCmdResult> OpenOrder(bool isAysnc, string symbol, OrderType type, OrderSide side, double price, double volume, double? sl, double? tp, string comment, OrderExecOptions options, string tag)
        {
            return ExecTradeRequest(isAysnc, (id, cbk) => _executor.SendOpenOrder(cbk, id, symbol, type, side, price, volume, tp, sl, comment, options, tag));
        }

        public Task<OrderCmdResult> CancelOrder(bool isAysnc, string orderId, OrderSide side)
        {
            return ExecTradeRequest(isAysnc, (id, cbk) => _executor.SendCancelOrder(cbk, id, orderId, side));
        }

        public Task<OrderCmdResult> CloseOrder(bool isAysnc, string orderId, double? closeVolumeLots)
        {
            return ExecTradeRequest(isAysnc, (id, cbk) => _executor.SendCloseOrder(cbk, id, orderId, closeVolumeLots));
        }

        public Task<OrderCmdResult> CloseOrderBy(bool isAysnc, string orderId, string byOrderId)
        {
            return ExecDoubleOrderTradeRequest(isAysnc, (id, cbk) => _executor.SendCloseOrderBy(cbk, id, orderId, byOrderId));
        }

        public Task<OrderCmdResult> ModifyOrder(bool isAysnc, string orderId, string symbol, OrderType type, OrderSide side, double currentVolume, double price, double? sl, double? tp, string comment)
        {
            return ExecTradeRequest(isAysnc, (id, cbk) => _executor.SendModifyOrder(cbk, id, orderId, symbol, type, side, price, currentVolume, tp, sl, comment));
        }

        #endregion

        private Task<OrderCmdResult> ExecTradeRequest(bool isAsync, Action<string, CrossDomainCallback<OrderCmdResultCodes>> executorInvoke)
        {
            var resultTask = new TaskCompletionSource<OrderCmdResult>();

            string operationId = Guid.NewGuid().ToString();

            reportListeners.Add(operationId, rep =>
            {
                reportListeners.Remove(operationId);
                resultTask.TrySetResult(new TradeResultEntity(rep.ResultCode, new OrderAccessor(rep.OrderCopy)));
            });

            var callback = new CrossDomainCallback<OrderCmdResultCodes>(code =>
            {
                if (code != OrderCmdResultCodes.Ok)
                    context.EnqueueTradeUpdate(b => InvokeListener(operationId, new OrderExecReport() { ResultCode = code }));
            });

            executorInvoke(operationId, callback);

            if (!isAsync)
            {
                while (!resultTask.Task.IsCompleted)
                    context.ProcessNextOrderUpdate();
            }

            return resultTask.Task;
        }

        private Task<OrderCmdResult> ExecDoubleOrderTradeRequest(bool isAsync, Action<string, CrossDomainCallback<OrderCmdResultCodes>> executorInvoke)
        {
            var resultTask = new TaskCompletionSource<OrderCmdResult>();
            var resultContainer = new List<OrderEntity>(2);

            string operationId = Guid.NewGuid().ToString();

            reportListeners.Add(operationId, rep =>
            {
                resultContainer.Add(rep.OrderCopy);
                if (resultContainer.Count == 2)
                {
                    reportListeners.Remove(operationId);
                    resultTask.TrySetResult(new TradeResultEntity(rep.ResultCode, new OrderAccessor(rep.OrderCopy)));
                }
            });

            var callback = new CrossDomainCallback<OrderCmdResultCodes>(code =>
            {
                if (code != OrderCmdResultCodes.Ok)
                    context.EnqueueTradeUpdate(b => InvokeListener(operationId, new OrderExecReport() { ResultCode = code }));
            });

            executorInvoke(operationId, callback);

            if (!isAsync)
            {
                while (!resultTask.Task.IsCompleted)
                    context.ProcessNextOrderUpdate();
            }

            return resultTask.Task;
        }

        private Task<OrderCmdResult> CreateResult(OrderCmdResultCodes code)
        {
            return Task.FromResult<OrderCmdResult>(new TradeResultEntity(code));
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
