using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    internal class TradingFixture : CrossDomainObject, TradeCommands
    {
        private IFixtureContext context;
        private Dictionary<string, Currency> currencies;
        private PluginLoggerAdapter _logger;
        private AccountEntity _account;
        private SymbolsCollection _symbols;

        public TradingFixture(IFixtureContext context)
        {
            this.context = context;
        }

        public IAccountInfoProvider DataProvider { get; set; }

        public void Start()
        {
            if (DataProvider != null)
                DataProvider.SyncInvoke(Init);
        }

        private void Init()
        {
            var builder = context.Builder;

            _logger = (PluginLoggerAdapter)builder.Logger;
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
            }
            
            foreach (var order in DataProvider.GetOrders())
                builder.Account.Orders.Add(order);
            foreach (var asset in DataProvider.GetAssets())
                builder.Account.Assets.Update(asset);
        }

        public void Stop()
        {
            DataProvider.SyncInvoke(Deinit);
        }

        private void Deinit()
        {
            DataProvider.OrderUpdated -= DataProvider_OrderUpdated;
        }

        private static void ApplyOrderEntity(OrderExecReport eReport, OrdersCollection collection)
        {
            if (eReport.Action == OrderEntityAction.Added)
                collection.Add(eReport.OrderCopy);
            else if (eReport.Action == OrderEntityAction.Removed)
                collection.Remove(eReport.OrderId);
            else if (eReport.Action == OrderEntityAction.Updated)
                collection.Replace(eReport.OrderCopy);
        }

        private void DataProvider_BalanceUpdated(BalanceOperationReport report)
        {
            var accProxy = context.Builder.Account;

            if (accProxy.Type == Api.AccountTypes.Gross || accProxy.Type == Api.AccountTypes.Net)
            {
                accProxy.Balance = report.Balance;
                accProxy.FireBalanceUpdateEvent();
            }
            else if (accProxy.Type == Api.AccountTypes.Cash)
            {
                accProxy.Assets.Update(new AssetEntity(report.Balance, report.CurrencyCode, currencies));
                accProxy.Assets.FireModified(new AssetUpdateEventArgsImpl(new AssetEntity(report.Balance, report.CurrencyCode, currencies)));
            }
        }

        private void DataProvider_PositionUpdated(PositionExecReport report)
        {
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
                ApplyOrderEntity(eReport, orderCollection);
                orderCollection.FireOrderOpened(new OrderOpenedEventArgsImpl(eReport.OrderCopy));
            }
            else if (eReport.ExecAction == OrderExecAction.Closed)
            {
                var oldOrder = orderCollection.GetOrderOrNull(eReport.OrderId);
                if (oldOrder != null)
                {
                    ApplyOrderEntity(eReport, orderCollection);
                    orderCollection.FireOrderClosed(new OrderClosedEventArgsImpl(eReport.OrderCopy));
                }
            }
            else if (eReport.ExecAction == OrderExecAction.Canceled)
            {
                var oldOrder = orderCollection.GetOrderOrNull(eReport.OrderId);
                if (oldOrder != null)
                {
                    ApplyOrderEntity(eReport, orderCollection);
                    orderCollection.FireOrderCanceled(new OrderCanceledEventArgsImpl(eReport.OrderCopy));
                }
            }
            else if (eReport.ExecAction == OrderExecAction.Expired)
            {
                var oldOrder = orderCollection.GetOrderOrNull(eReport.OrderId);
                if (oldOrder != null)
                {
                    ApplyOrderEntity(eReport, orderCollection);
                    orderCollection.FireOrderExpired(new OrderCanceledEventArgsImpl(eReport.OrderCopy));
                }
            }
            else if (eReport.ExecAction == OrderExecAction.Modified)
            {
                var oldOrder = orderCollection.GetOrderOrNull(eReport.OrderId);
                if (oldOrder != null && eReport.OrderCopy != null)
                {
                    ApplyOrderEntity(eReport, orderCollection);
                    orderCollection.FireOrderModified(new OrderModifiedEventArgsImpl(oldOrder, eReport.OrderCopy));
                }
            }
            else if (eReport.ExecAction == OrderExecAction.Filled)
            {
                var oldOrder = orderCollection.GetOrderOrNull(eReport.OrderId);
                if (oldOrder != null && eReport.OrderCopy != null)
                {
                    ApplyOrderEntity(eReport, orderCollection);
                    orderCollection.FireOrderFilled(new OrderFilledEventArgsImpl(oldOrder, eReport.OrderCopy));
                }
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
                    acc.FireBalanceUpdateEvent();
                }
            }
            else if (acc.Type == Api.AccountTypes.Cash)
            {
                if (eReport.Assets != null)
                {
                    foreach (var asset in eReport.Assets)
                    {
                        acc.Assets.Update(new AssetEntity(asset.Volume, asset.Currency, currencies));
                        acc.Assets.FireModified(new AssetUpdateEventArgsImpl(new AssetEntity(asset.Volume, asset.Currency, currencies)));
                    }
                }
            }
        }

        #region TradeCommands impl

        public async Task<OrderCmdResult> OpenOrder(bool isAysnc, string symbol, OrderType type, OrderSide side, double volumeLots, double price, double? sl, double? tp, string comment, OrderExecOptions options, string tag)
        {
            var smbMetadata = _symbols.List[symbol];
            if (smbMetadata.IsNull)
                return new TradeResultEntity(OrderCmdResultCodes.SymbolNotFound);

            volumeLots = RoundVolume(volumeLots, smbMetadata);
            double volume = ConvertVolume(volumeLots, smbMetadata);
            price = RoundPrice(price, smbMetadata, side);
            sl = RoundPrice(sl, smbMetadata, side);
            tp = RoundPrice(tp, smbMetadata, side);

            LogOrderOpening(symbol, type, side, volumeLots, price, sl, tp);

            using (var waitHandler = new TaskProxy<OpenModifyResult>())
            {
                api.OpenOrder(waitHandler, symbol, type, side, price, volume, tp, sl, comment, options, tag);
                var result = await waitHandler.LocalTask.ConfigureAwait(isAysnc);

                TradeResultEntity resultEntity;
                if (result.ResultCode == OrderCmdResultCodes.Ok)
                {
                    _account.Orders.Add(result.NewOrder);
                    resultEntity = new TradeResultEntity(result.ResultCode, result.NewOrder);
                }
                else
                {
                    var orderToOpen = new OrderEntity("-1")
                    {
                        Symbol = symbol,
                        Type = type,
                        Side = side,
                        RemainingVolume = volumeLots,
                        RequestedVolume = volumeLots,
                        Price = price,
                        StopLoss = sl ?? double.NaN,
                        TakeProfit = tp ?? double.NaN,
                        Comment = comment,
                        Tag = tag
                    };
                    resultEntity = new TradeResultEntity(result.ResultCode, orderToOpen);
                }

                LogOrderOpenResults(resultEntity);

                return resultEntity;
            }
        }

        public async Task<OrderCmdResult> CancelOrder(bool isAysnc, string orderId)
        {
            Order orderToCancel = _account.Orders.GetOrderOrNull(orderId);
            if (orderToCancel == null)
                return new TradeResultEntity(OrderCmdResultCodes.OrderNotFound);

            _logger.PrintTrade("Canceling order #" + orderId);

            using (var waitHandler = new TaskProxy<CancelResult>())
            {
                api.CancelOrder(waitHandler, orderId, ((OrderEntity)orderToCancel).ClientOrderId, orderToCancel.Side);
                var result = await waitHandler.LocalTask.ConfigureAwait(isAysnc);

                if (result.ResultCode == OrderCmdResultCodes.Ok)
                {
                    _account.Orders.Remove(orderId);
                    _logger.PrintTrade("→ SUCCESS: Order #" + orderId + " canceled");
                }
                else
                    _logger.PrintTrade("→ FAILED Canceling order #" + orderId + " error=" + result.ResultCode);

                return new TradeResultEntity(result.ResultCode, orderToCancel);
            }
        }

        public async Task<OrderCmdResult> CloseOrder(bool isAysnc, string orderId, double? closeVolumeLots)
        {
            double? closeVolume = null;

            Order orderToClose = _account.Orders.GetOrderOrNull(orderId);
            if (orderToClose == null)
                return new TradeResultEntity(OrderCmdResultCodes.OrderNotFound);

            if (closeVolumeLots != null)
            {
                var smbMetadata = _symbols.List[orderToClose.Symbol];
                if (smbMetadata.IsNull)
                    return new TradeResultEntity(OrderCmdResultCodes.SymbolNotFound);

                closeVolumeLots = RoundVolume(closeVolumeLots, smbMetadata);
                closeVolume = ConvertVolume(closeVolumeLots.Value, smbMetadata);
            }

            _logger.PrintTrade("Closing order #" + orderId);

            using (var waitHandler = new TaskProxy<CloseResult>())
            {
                api.CloseOrder(waitHandler, orderId, closeVolume);
                var result = await waitHandler.LocalTask.ConfigureAwait(isAysnc);

                if (result.ResultCode == OrderCmdResultCodes.Ok)
                {
                    var orderClone = new OrderEntity(orderToClose);
                    orderClone.RemainingVolume -= result.ExecVolume;

                    if (orderClone.RemainingVolume <= 0)
                        _account.Orders.Remove(orderId);
                    else
                        _account.Orders.Replace(orderClone);

                    _logger.PrintTrade("→ SUCCESS: Order #" + orderId + " closed");

                    return new TradeResultEntity(result.ResultCode, orderClone);
                }
                else
                {
                    _logger.PrintTrade("→ FAILED Closing order #" + orderId + " error=" + result.ResultCode);
                    return new TradeResultEntity(result.ResultCode, orderToClose);
                }
            }
        }

        public async Task<OrderCmdResult> ModifyOrder(bool isAysnc, string orderId, double price, double? sl, double? tp, string comment)
        {
            Order orderToModify = _account.Orders.GetOrderOrNull(orderId);
            if (orderToModify == null)
                return new TradeResultEntity(OrderCmdResultCodes.OrderNotFound);

            var smbMetadata = _symbols.List[orderToModify.Symbol];
            if (smbMetadata.IsNull)
                return new TradeResultEntity(OrderCmdResultCodes.SymbolNotFound);

            double orderVolume = ConvertVolume(orderToModify.RequestedVolume, smbMetadata);
            price = RoundPrice(price, smbMetadata, orderToModify.Side);
            sl = RoundPrice(sl, smbMetadata, orderToModify.Side);
            tp = RoundPrice(tp, smbMetadata, orderToModify.Side);

            _logger.PrintTrade("Modifying order #" + orderId);

            using (var waitHandler = new TaskProxy<OpenModifyResult>())
            {
                api.ModifyOrder(waitHandler, orderId, ((OrderEntity)orderToModify).ClientOrderId, orderToModify.Symbol, orderToModify.Type, orderToModify.Side,
                    price, orderVolume, tp, sl, comment);
                var result = await waitHandler.LocalTask.ConfigureAwait(isAysnc);

                if (result.ResultCode == OrderCmdResultCodes.Ok)
                {
                    _account.Orders.Replace(result.NewOrder);
                    _logger.PrintTrade("→ SUCCESS: Order #" + orderId + " modified");
                    return new TradeResultEntity(result.ResultCode, result.NewOrder);
                }
                else
                {
                    _logger.PrintTrade("→ FAILED Modifying order #" + orderId + " error=" + result.ResultCode);
                    return new TradeResultEntity(result.ResultCode, orderToModify);
                }
            }
        }

        #endregion

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

        #region Logging

        private void LogOrderOpening(string symbol, OrderType type, OrderSide side, double volumeLots, double price, double? sl, double? tp)
        {
            StringBuilder logEntry = new StringBuilder();
            logEntry.Append("Executing ");
            AppendOrderParams(logEntry, " Order to ", symbol, type, side, volumeLots, price, sl, tp);
            _logger.PrintTrade(logEntry.ToString());
        }

        private void LogOrderOpenResults(OrderCmdResult result)
        {
            var order = result.ResultingOrder;
            StringBuilder logEntry = new StringBuilder();

            if (result.IsCompleted)
            {
                logEntry.Append("→ SUCCESS: Opened ");
                if (order != null)
                {
                    if (!double.IsNaN(order.LastFillPrice))
                    {
                        logEntry.Append("#").Append(order.Id).Append(" ");
                        AppendOrderParams(logEntry, " ", order.Symbol, order.Type, order.Side,
                            order.LastFillVolume, order.LastFillPrice, order.StopLoss, order.TakeProfit);
                    }
                    else
                    {
                        logEntry.Append("#").Append(order.Id).Append(" ");
                        AppendOrderParams(logEntry, " ", order.Symbol, order.Type, order.Side,
                            order.RemainingVolume, order.Price, order.StopLoss, order.TakeProfit);
                    }

                }
                else
                    logEntry.Append("Null Order");
            }
            else
            {
                logEntry.Append("→ FAILED Executing ");
                if (order != null)
                {
                    AppendOrderParams(logEntry, " Order to ", order.Symbol, order.Type, order.Side,
                        order.RemainingVolume, order.Price, order.StopLoss, order.TakeProfit);
                    logEntry.Append(" error=").Append(result.ResultCode);
                }
                else
                    logEntry.Append("Null Order");
            }

            _logger.PrintTrade(logEntry.ToString());
        }

        private void AppendOrderParams(StringBuilder logEntry, string sufix, string symbol, OrderType type, OrderSide side, double volumeLots, double price, double? sl, double? tp)
        {
            logEntry.Append(type)
                .Append(sufix).Append(side)
                .Append(" ").Append(volumeLots)
                .Append(" ").Append(symbol);

            if (tp != null || sl != null)
            {
                logEntry.Append(" (");
                if (sl != null)
                    logEntry.Append("SL:").Append(sl.Value);
                if (sl != null && tp != null)
                    logEntry.Append(", ");
                if (tp != null)
                    logEntry.Append("TP:").Append(tp.Value);

                logEntry.Append(")");
            }

            if (!double.IsNaN(price) && price != 0)
                logEntry.Append(" at price ").Append(price);
        }

        #endregion
    }
}
