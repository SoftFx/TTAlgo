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

        public Task<OrderCmdResult> CancelOrder(CancelOrdeRequest request)
        {
            Task<OrderCmdResult> task = new Task<OrderCmdResult>(() =>
            {
                try
                {
                    conenction.TradeProxy.Server.DeletePendingOrder(
                         request.OrderId.ToString(),
                         null,
                         Convert(request.Side)
                         );
                    return new TradeResultEntity(OrderCmdResultCodes.Ok, null);
                }
                catch (Exception)
                {
                    return new TradeResultEntity(OrderCmdResultCodes.DealerReject, null);
                }
            });

            orderQueue.Post(task);
            return task;
        }

        public Task<OrderCmdResult> CloseOrder(CloseOrdeRequest request)
        {
            Task<OrderCmdResult> task = new Task<OrderCmdResult>(() =>
            {
                try
                {
                    var result = conenction.TradeProxy.Server.ClosePositionPartially(
                         request.OrderId.ToString(),
                         Convert(request.Volume));
                    return new TradeResultEntity(OrderCmdResultCodes.Ok, null);
                }
                catch (Exception)
                {
                    return new TradeResultEntity(OrderCmdResultCodes.DealerReject, null);
                }
            });

            orderQueue.Post(task);
            return task;
        }

        public Task<OrderCmdResult> ModifyOrder(ModifyOrdeRequest request)
        {
            Task<OrderCmdResult> task = new Task<OrderCmdResult>(() =>
            {
                try
                {
                    var result = conenction.TradeProxy.Server.ModifyTradeRecord(
                         request.OrderId.ToString(),
                         null,
                         request.SymbolCode,
                         ToRecorType(request.OrderType),
                         Convert(request.Side),
                         Convert(request.Volume),
                         request.Price,
                         request.StopLoss,
                         request.TaskProfit,
                         null,
                         request.Comment);
                    return new TradeResultEntity(OrderCmdResultCodes.Ok, null);
                }
                catch (Exception)
                {
                    return new TradeResultEntity(OrderCmdResultCodes.DealerReject, null);
                }
            });

            orderQueue.Post(task);
            return task;
        }

        public void OpenOrder(OpenOrdeRequest request, TaskProxy<OrderCmdResult> waitHandler)
        {
            Task<OrderCmdResult> task = new Task<OrderCmdResult>(() =>
            {
                try
                {
                    var record = conenction.TradeProxy.Server.SendOrder(
                         request.SymbolCode,
                         Convert(request.OrderType),
                         Convert(request.Side),
                         request.Price,
                         request.Volume,
                         request.StopLoss,
                         request.TaskProfit,
                         null,
                         request.Comment
                         );
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

        private double Convert(OrderVolume vol)
        {
            return vol.Value;
        }

        private TradeCommand Convert(OrderTypes type)
        {
            switch (type)
            {
                case OrderTypes.Limit: return TradeCommand.Limit;
                case OrderTypes.Market: return TradeCommand.Market;
                case OrderTypes.Stop: return TradeCommand.Stop;
            }

            throw new Exception("Not Supported: " + type);
        }

        private TradeRecordType ToRecorType(OrderTypes type)
        {
            switch (type)
            {
                case OrderTypes.Limit: return TradeRecordType.Limit;
                case OrderTypes.Market: return TradeRecordType.Market;
                case OrderTypes.Stop: return TradeRecordType.Stop;
                case OrderTypes.Position: return TradeRecordType.Position;
            }

            throw new Exception("Not Supported: " + type);
        }

        private TradeRecordSide Convert(OrderSides side)
        {
            switch (side)
            {
                case OrderSides.Buy: return TradeRecordSide.Buy;
                case OrderSides.Sell: return TradeRecordSide.Sell;
            }

            throw new Exception("Not Supported: " + side);
        }
    }
}
