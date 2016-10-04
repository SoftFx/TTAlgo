using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotTerminal
{
    internal class TradeExecutor : NoTimeoutByRefObject, ITradeApi
    {
        private static readonly NLog.ILogger logger = NLog.LogManager.GetCurrentClassLogger();

        private BufferBlock<Task> orderQueue;
        private ActionBlock<Task> orderSender;
        private ConnectionModel conenction;

        public TradeExecutor(TraderClientModel trader)
        {
            this.conenction = trader.Connection;

            orderQueue = new BufferBlock<Task>();

            var senderOptions = new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 10 };
            orderSender = new ActionBlock<Task>(t => t.RunSynchronously(), senderOptions);

            orderQueue.LinkTo(orderSender);
        }

        public void OpenOrder(TaskProxy<OrderCmdResult> waitHandler, string symbol,
            OrderType type, OrderSide side, double price, double volume, double? tp, double? sl, string comment)
        {
            Task<OrderCmdResult> task = new Task<OrderCmdResult>(() =>
            {
                try
                {
                    var record = conenction.TradeProxy.Server.SendOrder(symbol, Convert(type), Convert(side),
                        price, volume, sl, tp, null, comment);
                    return new TradeResultEntity(OrderCmdResultCodes.Ok, new OrderModel(record).ToAlgoOrder());
                }
                catch (SoftFX.Extended.Errors.RejectException rex)
                {
                    return new TradeResultEntity(Convert(rex.Reason), null);
                }
                catch (SoftFX.Extended.Errors.LogoutException)
                {
                    return new TradeResultEntity(OrderCmdResultCodes.ConnectionError, null);
                }
                catch (SoftFX.Extended.Errors.TimeoutException)
                {
                    return new TradeResultEntity(OrderCmdResultCodes.Timeout, null);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "OpenOrder() failed!");
                    return new TradeResultEntity(OrderCmdResultCodes.InternalError, null);
                }
            });

            waitHandler.Attach(task);
            orderQueue.Post(task);
        }

        public void CancelOrder(TaskProxy<OrderCmdResult> waitHandler, string orderId, string clientOrderId, OrderSide side)
        {
            Task<OrderCmdResult> task = new Task<OrderCmdResult>(() =>
            {
                try
                {
                    conenction.TradeProxy.Server.DeletePendingOrder(orderId, clientOrderId, Convert(side));
                    return new TradeResultEntity(OrderCmdResultCodes.Ok, null);
                }
                catch (SoftFX.Extended.Errors.RejectException rex)
                {
                    return new TradeResultEntity(Convert(rex.Reason), null);
                }
                catch (SoftFX.Extended.Errors.LogoutException)
                {
                    return new TradeResultEntity(OrderCmdResultCodes.ConnectionError, null);
                }
                catch (SoftFX.Extended.Errors.TimeoutException)
                {
                    return new TradeResultEntity(OrderCmdResultCodes.Timeout, null);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "CancelOrder() failed!");
                    return new TradeResultEntity(OrderCmdResultCodes.InternalError, null);
                }
            });

            waitHandler.Attach(task);
            orderQueue.Post(task);
        }

        public void ModifyOrder(TaskProxy<OrderCmdResult> waitHandler, string orderId, string clientOrderId, string symbol,
            OrderType orderType, OrderSide side, double price, double volume, double? tp, double? sl, string comment)
        {
            Task<OrderCmdResult> task = new Task<OrderCmdResult>(() =>
            {
                try
                {
                    var result = conenction.TradeProxy.Server.ModifyTradeRecord(orderId, clientOrderId, symbol,
                        ToRecordType(orderType), Convert(side), volume, price, sl, tp, null, comment);
                    return new TradeResultEntity(OrderCmdResultCodes.Ok, null);
                }
                catch (SoftFX.Extended.Errors.RejectException rex)
                {
                    return new TradeResultEntity(Convert(rex.Reason), null);
                }
                catch (SoftFX.Extended.Errors.LogoutException)
                {
                    return new TradeResultEntity(OrderCmdResultCodes.ConnectionError, null);
                }
                catch (SoftFX.Extended.Errors.TimeoutException)
                {
                    return new TradeResultEntity(OrderCmdResultCodes.Timeout, null);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "ModifyOrder() failed!");
                    return new TradeResultEntity(OrderCmdResultCodes.InternalError, null);
                }
            });

            waitHandler.Attach(task);
            orderQueue.Post(task);
        }

        public void CloseOrder(TaskProxy<OrderCmdResult> waitHandler, string orderId, double? volume)
        {
            var task = new Task<OrderCmdResult>(() =>
            {
                try
                {
                    if (volume == null)
                    {
                        var result = conenction.TradeProxy.Server.ClosePosition(orderId);
                        return new TradeResultEntity(OrderCmdResultCodes.Ok, null);
                    }
                    else
                    {
                        var result = conenction.TradeProxy.Server.ClosePositionPartially(orderId, volume.Value);
                        return new TradeResultEntity(OrderCmdResultCodes.Ok, null);
                    }
                }
                catch (SoftFX.Extended.Errors.RejectException rex)
                {
                    return new TradeResultEntity(Convert(rex.Reason), null);
                }
                catch (SoftFX.Extended.Errors.LogoutException)
                {
                    return new TradeResultEntity(OrderCmdResultCodes.ConnectionError, null);
                }
                catch (SoftFX.Extended.Errors.TimeoutException)
                {
                    return new TradeResultEntity(OrderCmdResultCodes.Timeout, null);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "CloseOrder() failed!");
                    return new TradeResultEntity(OrderCmdResultCodes.InternalError, null);
                }
            });

            waitHandler.Attach(task);
            orderQueue.Post(task);
        }

        private TradeCommand Convert(OrderType type)
        {
            switch (type)
            {
                case OrderType.Limit: return TradeCommand.Limit;
                case OrderType.Market: return TradeCommand.Market;
                case OrderType.Stop: return TradeCommand.Stop;
            }

            throw new Exception("Not Supported: " + type);
        }

        private TradeRecordType ToRecordType(OrderType type)
        {
            switch (type)
            {
                case OrderType.Limit: return TradeRecordType.Limit;
                case OrderType.Market: return TradeRecordType.Market;
                case OrderType.Stop: return TradeRecordType.Stop;
                case OrderType.Position: return TradeRecordType.Position;
            }

            throw new Exception("Not Supported: " + type);
        }

        private TradeRecordSide Convert(OrderSide side)
        {
            switch (side)
            {
                case OrderSide.Buy: return TradeRecordSide.Buy;
                case OrderSide.Sell: return TradeRecordSide.Sell;
            }

            throw new Exception("Not Supported: " + side);
        }

        private OrderCmdResultCodes Convert(RejectReason reason)
        {
            switch (reason)
            {
                case RejectReason.DealerReject: return OrderCmdResultCodes.DealerReject;
                case RejectReason.UnknownSymbol: return OrderCmdResultCodes.SymbolNotFound;
                case RejectReason.UnknownOrder: return OrderCmdResultCodes.OrderNotFound;
                case RejectReason.IncorrectQuantity: return OrderCmdResultCodes.IncorrectVolume;
                case RejectReason.OffQuotes: return OrderCmdResultCodes.Offquotes;
                default: return OrderCmdResultCodes.UnknownError;
            }
        }
    }
}
