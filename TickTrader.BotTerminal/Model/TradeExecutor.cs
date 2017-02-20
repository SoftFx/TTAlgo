using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotTerminal
{
    internal class TradeExecutor : CrossDomainObject, ITradeApi
    {
        private static readonly NLog.ILogger logger = NLog.LogManager.GetCurrentClassLogger();

        private BufferBlock<Task> orderQueue;
        private ActionBlock<Task> orderSender;
        private ConnectionModel conenction;
        private IOrderDependenciesResolver resolver;

        public TradeExecutor(TraderClientModel trader)
        {
            this.conenction = trader.Connection;
            this.resolver = trader.Symbols;

            orderQueue = new BufferBlock<Task>();

            var senderOptions = new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 10 };
            orderSender = new ActionBlock<Task>(t => t.RunSynchronously(), senderOptions);

            orderQueue.LinkTo(orderSender);
        }

        public void OpenOrder(TaskProxy<OpenModifyResult> waitHandler, string symbol,
            OrderType type, OrderSide side, double price, double volume, double? tp, double? sl, string comment, OrderExecOptions options, string tag)
        {
            var task = new Task<OpenModifyResult>(() =>
            {
                try
                {
                    ValidatePrice(price);
                    ValidateVolume(volume);
                    ValidateTp(tp);
                    ValidateSl(sl);

                    var record = conenction.TradeProxy.Server.SendOrder(symbol, Convert(type, options), Convert(side),
                        price, volume, null, null, sl, tp, null, comment, tag, null);
                    return new OpenModifyResult(OrderCmdResultCodes.Ok, new OrderModel(record, resolver).ToAlgoOrder());
                }
                catch (ValidatioException vex)
                {
                    return new OpenModifyResult(vex.Code, null);
                }
                catch (SoftFX.Extended.Errors.RejectException rex)
                {
                    return new OpenModifyResult(Convert(rex.Reason, rex.Message), null);
                }
                catch (SoftFX.Extended.Errors.LogoutException)
                {
                    return new OpenModifyResult(OrderCmdResultCodes.ConnectionError, null);
                }
                catch (SoftFX.Extended.Errors.TimeoutException)
                {
                    return new OpenModifyResult(OrderCmdResultCodes.Timeout, null);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "OpenOrder() failed!");
                    return new OpenModifyResult(OrderCmdResultCodes.InternalError, null);
                }
            });

            waitHandler.Attach(task);
            orderQueue.Post(task);
        }

        public void CancelOrder(TaskProxy<CancelResult> waitHandler, string orderId, string clientOrderId, OrderSide side)
        {
            var task = new Task<CancelResult>(() =>
            {
                try
                {
                    ValidateOrderId(orderId);

                    conenction.TradeProxy.Server.DeletePendingOrder(orderId, Convert(side));
                    return new CancelResult(OrderCmdResultCodes.Ok);
                }
                catch (ValidatioException vex)
                {
                    return new CancelResult(vex.Code);
                }
                catch (SoftFX.Extended.Errors.RejectException rex)
                {
                    return new CancelResult(Convert(rex.Reason, rex.Message));
                }
                catch (SoftFX.Extended.Errors.LogoutException)
                {
                    return new CancelResult(OrderCmdResultCodes.ConnectionError);
                }
                catch (SoftFX.Extended.Errors.TimeoutException)
                {
                    return new CancelResult(OrderCmdResultCodes.Timeout);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "CancelOrder() failed!");
                    return new CancelResult(OrderCmdResultCodes.InternalError);
                }
            });

            waitHandler.Attach(task);
            orderQueue.Post(task);
        }

        public void ModifyOrder(TaskProxy<OpenModifyResult> waitHandler, string orderId, string clientOrderId, string symbol,
            OrderType orderType, OrderSide side, double price, double volume, double? tp, double? sl, string comment)
        {
            var task = new Task<OpenModifyResult>(() =>
            {
                try
                {
                    ValidateOrderId(orderId);

                    var result = conenction.TradeProxy.Server.ModifyTradeRecord(orderId, symbol,
                        ToRecordType(orderType), Convert(side), volume, null, null, price, sl, tp, null, comment, null, null);
                    if (!string.IsNullOrEmpty(result.OrderId)) // Ugly hack to make it work!
                        return new OpenModifyResult(OrderCmdResultCodes.Ok, new OrderModel(result, resolver).ToAlgoOrder());
                    return new OpenModifyResult(OrderCmdResultCodes.DealerReject, null);
                }
                catch (ValidatioException vex)
                {
                    return new OpenModifyResult(vex.Code, null);
                }
                catch (SoftFX.Extended.Errors.RejectException rex)
                {
                    return new OpenModifyResult(Convert(rex.Reason, rex.Message), null);
                }
                catch (SoftFX.Extended.Errors.LogoutException)
                {
                    return new OpenModifyResult(OrderCmdResultCodes.ConnectionError, null);
                }
                catch (SoftFX.Extended.Errors.TimeoutException)
                {
                    return new OpenModifyResult(OrderCmdResultCodes.Timeout, null);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "ModifyOrder() failed!");
                    return new OpenModifyResult(OrderCmdResultCodes.InternalError, null);
                }
            });

            waitHandler.Attach(task);
            orderQueue.Post(task);
        }

        public void CloseOrder(TaskProxy<CloseResult> waitHandler, string orderId, double? volume)
        {
            var task = new Task<CloseResult>(() =>
            {
                try
                {
                    ValidateOrderId(orderId);

                    if (volume == null)
                    {
                        var result = conenction.TradeProxy.Server.ClosePosition(orderId);
                        return new CloseResult(OrderCmdResultCodes.Ok, result.ExecutedPrice, result.ExecutedVolume);
                    }
                    else
                    {
                        ValidateVolume(volume.Value);
                        var result = conenction.TradeProxy.Server.ClosePositionPartially(orderId, volume.Value);
                        return new CloseResult(OrderCmdResultCodes.Ok);
                    }
                }
                catch (ValidatioException vex)
                {
                    return new CloseResult(vex.Code);
                }
                catch (SoftFX.Extended.Errors.RejectException rex)
                {
                    return new CloseResult(Convert(rex.Reason, rex.Message));
                }
                catch (SoftFX.Extended.Errors.LogoutException)
                {
                    return new CloseResult(OrderCmdResultCodes.ConnectionError);
                }
                catch (SoftFX.Extended.Errors.TimeoutException)
                {
                    return new CloseResult(OrderCmdResultCodes.Timeout);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "CloseOrder() failed!");
                    return new CloseResult(OrderCmdResultCodes.InternalError);
                }
            });

            waitHandler.Attach(task);
            orderQueue.Post(task);
        }

        private TradeCommand Convert(OrderType type, OrderExecOptions options)
        {
            switch (type)
            {
                case OrderType.Limit:
                    if (options.IsFlagSet(OrderExecOptions.ImmediateOrCancel))
                        return TradeCommand.IoC;
                    else
                        return TradeCommand.Limit;
                case OrderType.Market: return TradeCommand.Market;
                case OrderType.Stop: return TradeCommand.Stop;
                case OrderType.StopLimit: return TradeCommand.StopLimit;
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
                case OrderType.StopLimit: return TradeRecordType.StopLimit;
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

        private OrderCmdResultCodes Convert(RejectReason reason, string message)
        {
            switch (reason)
            {
                case RejectReason.DealerReject: return OrderCmdResultCodes.DealerReject;
                case RejectReason.UnknownSymbol: return OrderCmdResultCodes.SymbolNotFound;
                case RejectReason.UnknownOrder: return OrderCmdResultCodes.OrderNotFound;
                case RejectReason.IncorrectQuantity: return OrderCmdResultCodes.IncorrectVolume;
                case RejectReason.OffQuotes: return OrderCmdResultCodes.OffQuotes;
                case RejectReason.OrderExceedsLImit: return OrderCmdResultCodes.NotEnoughMoney;
                case RejectReason.Other:
                    {
                        if (message == "Trade Not Allowed")
                            return OrderCmdResultCodes.TradeNotAllowed;
                        break;
                    }
                case RejectReason.None:
                    {
                        if (message.StartsWith("Order Not Found"))
                            return OrderCmdResultCodes.OrderNotFound;
                        break;
                    }
            }
            return OrderCmdResultCodes.UnknownError;
        }

        private void ValidateVolume(double volume)
        {
            if (volume <= 0 || double.IsNaN(volume) || double.IsInfinity(volume))
                throw new ValidatioException(OrderCmdResultCodes.IncorrectVolume);
        }

        private void ValidatePrice(double price)
        {
            if (price <= 0 || double.IsNaN(price) || double.IsInfinity(price))
                throw new ValidatioException(OrderCmdResultCodes.IncorrectPrice);
        }

        private void ValidateTp(double? tp)
        {
            if (tp == null)
                return;

            if (tp.Value <= 0 || double.IsNaN(tp.Value) || double.IsInfinity(tp.Value))
                throw new ValidatioException(OrderCmdResultCodes.IncorrectTp);
        }

        private void ValidateSl(double? sl)
        {
            if (sl == null)
                return;

            if (sl.Value <= 0 || double.IsNaN(sl.Value) || double.IsInfinity(sl.Value))
                throw new ValidatioException(OrderCmdResultCodes.IncorrectSl);
        }

        private void ValidateOrderId(string orderId)
        {
            long parsedId;
            if (!long.TryParse(orderId, out parsedId))
                throw new ValidatioException(OrderCmdResultCodes.IncorrectOrderId);
        }

        private class ValidatioException : Exception
        {
            public ValidatioException(OrderCmdResultCodes code)
            {
                this.Code = code;
            }

            public OrderCmdResultCodes Code { get; private set; }
        }
    }
}
