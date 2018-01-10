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

namespace TickTrader.Algo.Common.Model.Interop
{
    internal class FdkAsyncExecutor
    {
        private static readonly IAlgoCoreLogger logger = CoreLoggerFactory.GetLogger<FdkAsyncExecutor>();

        private BufferBlock<Task> orderQueue;
        private ActionBlock<Task> orderSender;
        private DataTrade _tradeProxy;

        public FdkAsyncExecutor(DataTrade client)
        {
            this._tradeProxy = client;

            orderQueue = new BufferBlock<Task>();

            var senderOptions = new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 10 };
            orderSender = new ActionBlock<Task>(t => t.RunSynchronously(), senderOptions);

            orderQueue.LinkTo(orderSender);
        }

        public Task<OrderCmdResultCodes> SendOpenOrder(OpenOrderRequest request)
        {
            return EnqueueTradeOp("OpenOrder", () =>
            {
                var operationId = request.OperationId;
                var orderType = request.Type;
                var price = request.Price;
                var stopPrice = request.StopPrice;
                var maxVisibleVolume = request.MaxVisibleVolume;
                var volume = request.Volume;
                var tp = request.TakeProfit;
                var sl = request.StopLoss;

                if (orderType != OrderType.Stop && orderType != OrderType.Market)
                {
                    ValidatePrice(price);
                    ValidateMaxVisibleVolume(maxVisibleVolume);
                }

                if (orderType == OrderType.Stop || orderType == OrderType.StopLimit)
                {
                    ValidateStopPrice(stopPrice);
                }

                ValidateVolume(volume);

                ValidateTp(tp);
                ValidateSl(sl);

                var px = orderType == OrderType.Stop ? default(double?) : price;
                var stopPx = orderType == OrderType.Stop || orderType == OrderType.StopLimit ? stopPrice : default(double?);
                var maxVisVolume = orderType == OrderType.Limit || orderType == OrderType.StopLimit ? maxVisibleVolume : default(double?);

                _tradeProxy.Server.SendOrderEx(operationId, request.Symbol, Convert(orderType, request.Options), Convert(request.Side),
                    volume, maxVisVolume, px, stopPx, sl, tp, request.Expiration, request.Comment, request.Tag, null);
            });
        }

        public Task<OrderCmdResultCodes> SendCancelOrder(CancelOrderRequest request)
        {
            return EnqueueTradeOp("CancelOrder", () =>
            {
                var orderId = request.OrderId;
                var operationId = request.OperationId;
                var side = request.Side;

                ValidateOrderId(orderId);
                _tradeProxy.Server.DeletePendingOrderEx(operationId, orderId, Convert(side));
            });
        }

        public Task<OrderCmdResultCodes> SendModifyOrder(ReplaceOrderRequest request)
        {
            return EnqueueTradeOp("ModifyOrder", () =>
            {
                var orderId = request.OrderId;
                var orderType = request.Type;
                var price = request.Price;
                var stopPrice = request.StopPrice;
                var maxVisibleVolume = request.MaxVisibleVolume;

                ValidateOrderId(orderId);

                var px = orderType == OrderType.Stop ? default(double?) : price;
                var stopPx = orderType == OrderType.Stop || orderType == OrderType.StopLimit ? stopPrice : default(double?);
                var maxVisVolume = orderType == OrderType.Limit || orderType == OrderType.StopLimit ? maxVisibleVolume : default(double?);

                _tradeProxy.Server.ModifyTradeRecordEx(request.OperationId, orderId, request.Symbol,
                    ToRecordType(orderType), Convert(request.Side), request.CurrentVolume, maxVisVolume, px, stopPx,
                    request.StopLoss, request.TrakeProfit, request.Expiration, request.Comment, null, null);
            });
        }

        public Task<OrderCmdResultCodes> SendCloseOrder(CloseOrderRequest request)
        {
            return EnqueueTradeOp("CloseOrder", () =>
            {
                var orderId = request.OrderId;
                var byOrderId = request.ByOrderId;
                var volume = request.Volume;

                ValidateOrderId(orderId);

                if (byOrderId != null)
                {
                    ValidateOrderId(orderId);
                    ValidateOrderId(byOrderId);

                    var result = _tradeProxy.Server.CloseByPositionsEx(request.OperationId, orderId, byOrderId, -1);
                    if (!result)
                        throw new Exception("False! CloseByPositionsEx does not return error code! So enjoy this False by now.");
                }
                else if (volume == null)
                {
                    _tradeProxy.Server.ClosePositionEx(orderId, request.OperationId);
                }
                else
                {
                    ValidateVolume(volume.Value);
                    _tradeProxy.Server.ClosePositionPartiallyEx(orderId, volume.Value, request.OperationId);
                }
            });
        }

        private Task<OrderCmdResultCodes> EnqueueTradeOp(string opName, Action tradeOpDef)
        {
            return EnqueueTask(() =>
            {
                var result = HandleErrors(opName, tradeOpDef);
                return result;
            });
        }

        private OrderCmdResultCodes HandleErrors(string opName, Action tradeAction)
        {
            try
            {
                tradeAction();
                return OrderCmdResultCodes.Ok;
            }
            catch (ValidationException vex)
            {
                return vex.Code;
            }
            catch (SoftFX.Extended.Errors.RejectException rex)
            {
                return FdkConvertor.Convert(rex.Reason, rex.Message);
            }
            catch (SoftFX.Extended.Errors.LogoutException)
            {
                return OrderCmdResultCodes.ConnectionError;
            }
            catch (SoftFX.Extended.Errors.TimeoutException)
            {
                return OrderCmdResultCodes.Timeout;
            }
            catch (Exception ex)
            {
                logger.Error(ex, opName + "() failed!");
                return OrderCmdResultCodes.InternalError;
            }
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
                case OrderType.StopLimit:
                    if (options.IsFlagSet(OrderExecOptions.ImmediateOrCancel))
                        return TradeCommand.StopLimit_IoC;
                    else
                        return TradeCommand.StopLimit;
                    ;
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

        private Task EnqueueTask(Action taskDef)
        {
            var task = new Task(taskDef);
            orderQueue.Post(task);
            return task;
        }

        private Task<T> EnqueueTask<T>(Func<T> taskDef)
        {
            var task = new Task<T>(taskDef);
            orderQueue.Post(task);
            return task;
        }

        private void ValidateVolume(double volume)
        {
            if (volume <= 0 || double.IsNaN(volume) || double.IsInfinity(volume))
                throw new ValidationException(OrderCmdResultCodes.IncorrectVolume);
        }

        private void ValidateMaxVisibleVolume(double? volume)
        {
            if (!volume.HasValue)
                return;

            if (volume < 0 || double.IsNaN(volume.Value) || double.IsInfinity(volume.Value))
                throw new ValidationException(OrderCmdResultCodes.IncorrectMaxVisibleVolume);
        }

        private void ValidatePrice(double? price)
        {
            if (price == null || price <= 0 || double.IsNaN(price.Value) || double.IsInfinity(price.Value))
                throw new ValidationException(OrderCmdResultCodes.IncorrectPrice);
        }

        private void ValidateStopPrice(double? price)
        {
            if (price == null || price <= 0 || double.IsNaN(price.Value) || double.IsInfinity(price.Value))
                throw new ValidationException(OrderCmdResultCodes.IncorrectStopPrice);
        }

        private void ValidateTp(double? tp)
        {
            if (tp == null)
                return;

            if (tp.Value <= 0 || double.IsNaN(tp.Value) || double.IsInfinity(tp.Value))
                throw new ValidationException(OrderCmdResultCodes.IncorrectTp);
        }

        private void ValidateSl(double? sl)
        {
            if (sl == null)
                return;

            if (sl.Value <= 0 || double.IsNaN(sl.Value) || double.IsInfinity(sl.Value))
                throw new ValidationException(OrderCmdResultCodes.IncorrectSl);
        }

        private void ValidateOrderId(string orderId)
        {
            long parsedId;
            if (!long.TryParse(orderId, out parsedId))
                throw new ValidationException(OrderCmdResultCodes.IncorrectOrderId);
        }

        private class ValidationException : Exception
        {
            public ValidationException(OrderCmdResultCodes code)
            {
                this.Code = code;
            }

            public OrderCmdResultCodes Code { get; private set; }
        }
    }
}
