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
        private BufferBlock<Task> orderQueue;
        private ActionBlock<Task> orderSender;
        private ConnectionModel conenction;

        public TradeExecutor(TraderModel trader)
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
                    return new TradeResultEntity(OrderCmdResultCodes.Ok, null);
                }
                catch (Exception)
                {
                    return new TradeResultEntity(OrderCmdResultCodes.DealerReject, null);
                }
            });

            waitHandler.Attach(task);
            orderQueue.Post(task);
        }

        public void CancelOrder(TaskProxy<OrderCmdResult> waitHandler, string orderId, OrderSide side)
        {
            Task<OrderCmdResult> task = new Task<OrderCmdResult>(() =>
            {
                try
                {
                    conenction.TradeProxy.Server.DeletePendingOrder(orderId, null, Convert(side));
                    return new TradeResultEntity(OrderCmdResultCodes.Ok, null);
                }
                catch (Exception)
                {
                    return new TradeResultEntity(OrderCmdResultCodes.DealerReject, null);
                }
            });

            waitHandler.Attach(task);
            orderQueue.Post(task);
        }

        public void ModifyOrder(TaskProxy<OrderCmdResult> waitHandler, string orderId, string symbol,
            OrderType orderType, OrderSide side, double price, double volume, double? tp, double? sl, string comment)
        {
            Task<OrderCmdResult> task = new Task<OrderCmdResult>(() =>
            {
                try
                {
                    var result = conenction.TradeProxy.Server.ModifyTradeRecord(orderId, null, symbol,
                        ToRecordType(orderType), Convert(side), volume, price, sl, tp, null, comment);
                    return new TradeResultEntity(OrderCmdResultCodes.Ok, null);
                }
                catch (Exception)
                {
                    return new TradeResultEntity(OrderCmdResultCodes.DealerReject, null);
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
                catch (Exception)
                {
                    return new TradeResultEntity(OrderCmdResultCodes.DealerReject, null);
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
    }
}
