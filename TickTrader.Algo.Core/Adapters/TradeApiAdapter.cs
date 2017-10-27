﻿using System;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;

namespace TickTrader.Algo.Core
{
    internal class TradeApiAdapter : TradeCommands
    {
        private ITradeApi api;
        private SymbolProvider symbols;
        private AccountEntity account;
        private PluginLoggerAdapter logger;
        private string _isolationTag;
        private ITradePermissions _permissions;

        public TradeApiAdapter(ITradeApi api, SymbolProvider symbols, AccountEntity account, PluginLoggerAdapter logger, ITradePermissions tradePermissions, string isolationTag)
        {
            this.api = api;
            this.symbols = symbols;
            this.account = account;
            this.logger = logger;
            this._isolationTag = isolationTag;
            this._permissions = tradePermissions;
        }

        [Obsolete]
        public async Task<OrderCmdResult> OpenOrder(bool isAysnc, string symbol, OrderType type, OrderSide side, double volumeLots, double price,
            double? sl, double? tp, string comment, OrderExecOptions options, string tag)
        {
            OrderCmdResult resultEntity;
            string isolationTag = CompositeTag.NewTag(_isolationTag, tag);

            var orderToOpen = new OrderEntity("-1")
            {
                Symbol = symbol,
                Type = type,
                Side = side,
                RemainingVolume = new TradeVolume(0, volumeLots),
                RequestedVolume = new TradeVolume(0, volumeLots),
                MaxVisibleVolume = new TradeVolume(double.NaN, double.NaN),
                Price = type != OrderType.Stop ? price : double.NaN,
                StopPrice = type == OrderType.Stop ? price : double.NaN,
                StopLoss = sl ?? double.NaN,
                TakeProfit = tp ?? double.NaN,
                Comment = comment,
                UserTag = tag,
                InstanceId = _isolationTag,
            };

            var smbMetadata = symbols.List[symbol];
            if (smbMetadata.IsNull)
            {
                resultEntity = new TradeResultEntity(OrderCmdResultCodes.SymbolNotFound, new OrderAccessor(orderToOpen));
            }
            else if (!_permissions.TradeAllowed)
            {
                resultEntity = new TradeResultEntity(OrderCmdResultCodes.TradeNotAllowed, new OrderAccessor(orderToOpen));
            }
            else if(!ValidateVolume(volumeLots, smbMetadata))
            {
                resultEntity = new TradeResultEntity(OrderCmdResultCodes.IncorrectVolume, new OrderAccessor(orderToOpen));
            }
            else
            {
                volumeLots = RoundVolume(volumeLots, smbMetadata);
                double volume = ConvertVolume(volumeLots, smbMetadata);
                price = RoundPrice(price, smbMetadata, side);
                sl = RoundPrice(sl, smbMetadata, side);
                tp = RoundPrice(tp, smbMetadata, side);
                LogOrderOpening(symbol, type, side, volumeLots, price, sl, tp);


                resultEntity = await api.OpenOrder(isAysnc, symbol, type, side, price, volume, tp, sl, comment, options, isolationTag);

                if (resultEntity.ResultCode != OrderCmdResultCodes.Ok)
                {
                    resultEntity = new TradeResultEntity(resultEntity.ResultCode, new OrderAccessor(orderToOpen));
                }
            }

            LogOrderOpenResults(resultEntity);
            return resultEntity;
        }


        public async Task<OrderCmdResult> OpenOrder(bool isAysnc, string symbol, OrderType orderType, OrderSide side, double volumeLots, double? maxVisibleVolumeLots, double? price, double? stopPrice,
            double? sl, double? tp, string comment, OrderExecOptions options, string tag)
        {
            OrderCmdResult resultEntity;
            string isolationTag = CompositeTag.NewTag(_isolationTag, tag);

            var orderToOpen = new OrderEntity("-1")
            {
                Symbol = symbol,
                Type = orderType,
                Side = side,
                RemainingVolume = new TradeVolume(0, volumeLots),
                RequestedVolume = new TradeVolume(0, volumeLots),
                MaxVisibleVolume = maxVisibleVolumeLots.HasValue ? new TradeVolume(0, maxVisibleVolumeLots.Value) : new TradeVolume(double.NaN, double.NaN),
                Price = price ?? double.NaN,
                StopPrice = stopPrice ?? double.NaN,
                StopLoss = sl ?? double.NaN,
                TakeProfit = tp ?? double.NaN,
                Comment = comment,
                UserTag = tag,
                InstanceId = _isolationTag,
            };

            var smbMetadata = symbols.List[symbol];
            if (smbMetadata.IsNull)
            {
                resultEntity = new TradeResultEntity(OrderCmdResultCodes.SymbolNotFound, new OrderAccessor(orderToOpen));
            }
            else if (!_permissions.TradeAllowed)
            {
                resultEntity = new TradeResultEntity(OrderCmdResultCodes.TradeNotAllowed, new OrderAccessor(orderToOpen));
            }
            else if (!ValidateVolume(volumeLots, smbMetadata))
            {
                resultEntity = new TradeResultEntity(OrderCmdResultCodes.IncorrectVolume, new OrderAccessor(orderToOpen));
            }
            else
            {
                volumeLots = RoundVolume(volumeLots, smbMetadata);
                double volume = ConvertVolume(volumeLots, smbMetadata);
                double? maxVisibleVolume = maxVisibleVolumeLots.HasValue ? ConvertVolume(maxVisibleVolumeLots.Value, smbMetadata) : maxVisibleVolumeLots;
                price = RoundPrice(price, smbMetadata, side);
                stopPrice = RoundPrice(stopPrice, smbMetadata, side);
                sl = RoundPrice(sl, smbMetadata, side);
                tp = RoundPrice(tp, smbMetadata, side);
                LogOrderOpening(symbol, orderType, side, volumeLots, (stopPrice ?? price) ?? double.NaN, sl, tp);


                resultEntity = await api.OpenOrder(isAysnc, symbol, orderType, side, price, stopPrice, volume, maxVisibleVolume, tp, sl, comment, options, isolationTag);

                if (resultEntity.ResultCode != OrderCmdResultCodes.Ok)
                {
                    resultEntity = new TradeResultEntity(resultEntity.ResultCode, new OrderAccessor(orderToOpen));
                }
            }

            LogOrderOpenResults(resultEntity);
            return resultEntity;
        }


        public async Task<OrderCmdResult> CancelOrder(bool isAysnc, string orderId)
        {
            if (!_permissions.TradeAllowed)
            {
                logger.PrintTrade("→ FAILED Canceling order #" + orderId + " error=" + OrderCmdResultCodes.TradeNotAllowed);
                return new TradeResultEntity(OrderCmdResultCodes.TradeNotAllowed);
            }

            var orderToCancel = account.Orders.GetOrderOrNull(orderId);
            if (orderToCancel == null)
                return new TradeResultEntity(OrderCmdResultCodes.OrderNotFound);

            logger.PrintTrade("Canceling order #" + orderId);

            var result = await api.CancelOrder(isAysnc, orderId, orderToCancel.Side);

            if (result.ResultCode == OrderCmdResultCodes.Ok)
                logger.PrintTrade("→ SUCCESS: Order #" + orderId + " canceled");
            else
                logger.PrintTrade("→ FAILED Canceling order #" + orderId + " error=" + result.ResultCode);

            return new TradeResultEntity(result.ResultCode, orderToCancel);
        }

        public async Task<OrderCmdResult> CloseOrder(bool isAysnc, string orderId, double? closeVolumeLots)
        {
            if (!_permissions.TradeAllowed)
            {
                logger.PrintTrade("→ FAILED Closing order #" + orderId + " error=" + OrderCmdResultCodes.TradeNotAllowed);
                return new TradeResultEntity(OrderCmdResultCodes.TradeNotAllowed);
            }

            double? closeVolume = null;

            var orderToClose = account.Orders.GetOrderOrNull(orderId);
            if (orderToClose == null)
                return new TradeResultEntity(OrderCmdResultCodes.OrderNotFound);

            var smbMetadata = symbols.List[orderToClose.Symbol];
            if (smbMetadata.IsNull)
                return new TradeResultEntity(OrderCmdResultCodes.SymbolNotFound);

            if (closeVolumeLots != null)
            {
                closeVolumeLots = RoundVolume(closeVolumeLots, smbMetadata);
                closeVolume = ConvertVolume(closeVolumeLots.Value, smbMetadata);
            }

            logger.PrintTrade("Closing order #" + orderId);

            var result = await api.CloseOrder(isAysnc, orderId, closeVolume);

            if (result.ResultCode == OrderCmdResultCodes.Ok)
            {
                logger.PrintTrade("→ SUCCESS: Order #" + orderId + " closed");
                return new TradeResultEntity(result.ResultCode, result.ResultingOrder);
            }
            else
            {
                logger.PrintTrade("→ FAILED Closing order #" + orderId + " error=" + result.ResultCode);
                return new TradeResultEntity(result.ResultCode, orderToClose);
            }
        }

        public async Task<OrderCmdResult> CloseOrderBy(bool isAysnc, string orderId, string byOrderId)
        {
            Order orderToClose = account.Orders.GetOrderOrNull(orderId);
            if (orderToClose == null)
                return new TradeResultEntity(OrderCmdResultCodes.OrderNotFound);

            Order orderByClose = account.Orders.GetOrderOrNull(byOrderId);
            if (orderByClose == null)
                return new TradeResultEntity(OrderCmdResultCodes.OrderNotFound);

            var result = await api.CloseOrderBy(isAysnc, orderId, byOrderId);

            if (result.ResultCode == OrderCmdResultCodes.Ok)
            {
                logger.PrintTrade("→ SUCCESS: Order #" + orderId + " closed by order #" + byOrderId);
                return new TradeResultEntity(result.ResultCode, orderToClose);
            }
            else
            {
                logger.PrintTrade("→ FAILED Closing order #" + orderId + " by order #" + byOrderId + " error=" + result.ResultCode);
                return new TradeResultEntity(result.ResultCode, orderToClose);
            }
        }
        [Obsolete]
        public async Task<OrderCmdResult> ModifyOrder(bool isAysnc, string orderId, double price, double? sl, double? tp, string comment)
        {
            if (!_permissions.TradeAllowed)
            {
                logger.PrintTrade("→ FAILED Modifying order #" + orderId + " error=" + OrderCmdResultCodes.TradeNotAllowed);
                return new TradeResultEntity(OrderCmdResultCodes.TradeNotAllowed);
            }

            var orderToModify = account.Orders.GetOrderOrNull(orderId);
            if (orderToModify == null)
                return new TradeResultEntity(OrderCmdResultCodes.OrderNotFound);

            var smbMetadata = symbols.List[orderToModify.Symbol];
            if (smbMetadata.IsNull)
            {
                logger.PrintTrade("→ FAILED Modifying order #" + orderId + " error=" + OrderCmdResultCodes.SymbolNotFound);
                return new TradeResultEntity(OrderCmdResultCodes.SymbolNotFound);
            }

            double orderVolume = ConvertVolume(orderToModify.RequestedVolume, smbMetadata);
            price = RoundPrice(price, smbMetadata, orderToModify.Side);
            sl = RoundPrice(sl, smbMetadata, orderToModify.Side);
            tp = RoundPrice(tp, smbMetadata, orderToModify.Side);

            logger.PrintTrade("Modifying order #" + orderId);

            var result = await api.ModifyOrder(isAysnc, orderId, orderToModify.Symbol, orderToModify.Type, orderToModify.Side,
                    orderVolume, price, sl, tp, comment);

            if (result.ResultCode == OrderCmdResultCodes.Ok)
            {
                logger.PrintTrade("→ SUCCESS: Order #" + orderId + " modified");
                return new TradeResultEntity(result.ResultCode, result.ResultingOrder);
            }
            else
            {
                logger.PrintTrade("→ FAILED Modifying order #" + orderId + " error=" + result.ResultCode);
                return new TradeResultEntity(result.ResultCode, orderToModify);
            }
        }

        public async Task<OrderCmdResult> ModifyOrder(bool isAysnc, string orderId, double? price, double? stopPrice, double? maxVisibleVolume, double? sl, double? tp, string comment)
        {
            if (!_permissions.TradeAllowed)
            {
                logger.PrintTrade("→ FAILED Modifying order #" + orderId + " error=" + OrderCmdResultCodes.TradeNotAllowed);
                return new TradeResultEntity(OrderCmdResultCodes.TradeNotAllowed);
            }

            var orderToModify = account.Orders.GetOrderOrNull(orderId);
            if (orderToModify == null)
                return new TradeResultEntity(OrderCmdResultCodes.OrderNotFound);

            var smbMetadata = symbols.List[orderToModify.Symbol];
            if (smbMetadata.IsNull)
            {
                logger.PrintTrade("→ FAILED Modifying order #" + orderId + " error=" + OrderCmdResultCodes.SymbolNotFound);
                return new TradeResultEntity(OrderCmdResultCodes.SymbolNotFound);
            }

            double orderVolume = ConvertVolume(orderToModify.RequestedVolume, smbMetadata);
            double? orderMaxVisibleVolume = maxVisibleVolume.HasValue ? ConvertVolume(maxVisibleVolume.Value, smbMetadata) : maxVisibleVolume;
            price = RoundPrice(price, smbMetadata, orderToModify.Side);
            stopPrice = RoundPrice(stopPrice, smbMetadata, orderToModify.Side);
            sl = RoundPrice(sl, smbMetadata, orderToModify.Side);
            tp = RoundPrice(tp, smbMetadata, orderToModify.Side);

            logger.PrintTrade("Modifying order #" + orderId);

            var result = await api.ModifyOrder(isAysnc, orderId, orderToModify.Symbol, orderToModify.Type, orderToModify.Side,
                    orderVolume, price, stopPrice, orderMaxVisibleVolume, sl, tp, comment);

            if (result.ResultCode == OrderCmdResultCodes.Ok)
            {
                logger.PrintTrade("→ SUCCESS: Order #" + orderId + " modified");
                return new TradeResultEntity(result.ResultCode, result.ResultingOrder);
            }
            else
            {
                logger.PrintTrade("→ FAILED Modifying order #" + orderId + " error=" + result.ResultCode);
                return new TradeResultEntity(result.ResultCode, orderToModify);
            }
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

        private TradeVolume ModifyVolume(TradeVolume oldVol, double byLots, Symbol smbInfo)
        {
            var lotSize = smbInfo.ContractSize;
            var byUnits = lotSize * byLots;
            return new TradeVolume(oldVol.Units - byUnits, oldVol.Lots - byLots);
        }

        private bool ValidateVolume(double volume, Symbol smbMetadata)
        {
            return volume.Gte(smbMetadata.MinTradeVolume) && volume.Lte(smbMetadata.MaxTradeVolume);
        }

        #region Logging

        private void LogOrderOpening(string symbol, OrderType type, OrderSide side, double volumeLots, double price, double? sl, double? tp)
        {
            StringBuilder logEntry = new StringBuilder();
            logEntry.Append("Opening ");
            AppendOrderParams(logEntry, " Order to ", symbol, type, side, volumeLots, price, sl, tp);
            logger.PrintTrade(logEntry.ToString());
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
                logEntry.Append("→ FAILED Opening ");
                if (order != null)
                {
                    AppendOrderParams(logEntry, " Order to ", order.Symbol, order.Type, order.Side,
                        order.RemainingVolume, order.Price, order.StopLoss, order.TakeProfit);
                    logEntry.Append(" error=").Append(result.ResultCode);
                }
                else
                    logEntry.Append("Null Order");
            }

            logger.PrintTrade(logEntry.ToString());
        }

        private void AppendOrderParams(StringBuilder logEntry, string sufix, string symbol, OrderType type, OrderSide side, double volumeLots, double price, double? sl, double? tp)
        {
            logEntry.Append(type)
                .Append(sufix).Append(side)
                .Append(" ").Append(volumeLots)
                .Append(" ").Append(symbol);

            if ((tp != null && !double.IsNaN(tp.Value)) || (sl != null && !double.IsNaN(sl.Value)))
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
