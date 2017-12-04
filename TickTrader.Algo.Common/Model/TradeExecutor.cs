using SoftFX.Extended;
using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Common.Model
{
    public class TradeExecutor : CrossDomainObject, ITradeExecutor
    {
        private static readonly IAlgoCoreLogger logger = CoreLoggerFactory.GetLogger<TradeExecutor>();

        private BufferBlock<Task> orderQueue;
        private ActionBlock<Task> orderSender;
        private ConnectionModel connection;
        private IOrderDependenciesResolver resolver;

        private DataTrade TradeProxy =>
            connection.State.Current == ConnectionModel.States.Online && connection.TradeProxy != null
            ? connection.TradeProxy
            : throw new ConnectionLostException();

        public TradeExecutor(ClientCore client)
        {
            this.connection = client.Connection;
            this.resolver = client.Symbols;

            orderQueue = new BufferBlock<Task>();

            var senderOptions = new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 10 };
            orderSender = new ActionBlock<Task>(t => t.RunSynchronously(), senderOptions);

            orderQueue.LinkTo(orderSender);
        }

        [Obsolete("Method is deprecated. There is no support for StopLimits and Hidden orders")]
        public void SendOpenOrder(CrossDomainCallback<OrderCmdResultCodes> callback, string operationId, string symbol,
            OrderType orderType, OrderSide side, double price, double volume, double? tp, double? sl, string comment, OrderExecOptions options, string tag)
        {
            if (orderType == OrderType.StopLimit)
                throw new NotSupportedException("StopLimit");

            var px = orderType == OrderType.Stop ? default(double?) : price;
            var stopPx = orderType == OrderType.Stop ? price : default(double?);

            SendOpenOrder(callback, operationId, symbol, orderType, side, px, stopPx, volume, null, tp, sl, comment, options, tag, null);
        }

        public void SendOpenOrder(CrossDomainCallback<OrderCmdResultCodes> callback, string operationId, string symbol, OrderType orderType, OrderSide side,
            double? price, double? stopPrice, double volume, double? maxVisibleVolume, double? tp, double? sl, string comment, OrderExecOptions options, string tag, DateTime? expiration)
        {
            EnqueueTradeOp("OpenOrder", callback, () =>
            {
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
                ValidateExpiration(expiration);

                var px = orderType == OrderType.Stop ? default(double?) : price;
                var stopPx = orderType == OrderType.Stop || orderType == OrderType.StopLimit ? stopPrice : default(double?);
                var maxVisVolume = orderType == OrderType.Limit || orderType == OrderType.StopLimit ? maxVisibleVolume : default(double?);

                TradeProxy.Server.SendOrderEx(operationId, symbol, Convert(orderType, options), Convert(side),
                    volume, maxVisVolume, px, stopPx, sl, tp, expiration, comment, tag, null);
            });
        }


        public void SendCancelOrder(CrossDomainCallback<OrderCmdResultCodes> callback, string operationId, string orderId, OrderSide side)
        {
            EnqueueTradeOp("CancelOrder", callback, () =>
            {
                ValidateOrderId(orderId);
                TradeProxy.Server.DeletePendingOrderEx(operationId, orderId, Convert(side));
            });
        }

        [Obsolete("Method is deprecated. There is no support for StopLimits and Hidden orders")]
        public void SendModifyOrder(CrossDomainCallback<OrderCmdResultCodes> callback, string operationId, string orderId, string symbol,
            OrderType orderType, OrderSide side, double price, double volume, double? tp, double? sl, string comment)
        {
            EnqueueTradeOp("ModifyOrder", callback, () =>
            {
                ValidateOrderId(orderId);

                var px = orderType == OrderType.Stop ? default(double?) : price;
                var stopPx = orderType == OrderType.Stop ? price : default(double?);

                TradeProxy.Server.ModifyTradeRecordEx(operationId, orderId, symbol,
                    ToRecordType(orderType), Convert(side), volume, null, px, stopPx, sl, tp, null, comment, null, null);
            });
        }

        public void SendModifyOrder(CrossDomainCallback<OrderCmdResultCodes> callback, string operationId, string orderId, string symbol, OrderType orderType, OrderSide side,
            double? price, double? stopPrice, double volume, double? maxVisibleVolume, double? tp, double? sl, string comment, DateTime? expiration)
        {
            EnqueueTradeOp("ModifyOrder", callback, () =>
            {
                ValidateOrderId(orderId);
                ValidateExpiration(expiration);

                var px = orderType == OrderType.Stop ? default(double?) : price;
                var stopPx = orderType == OrderType.Stop || orderType == OrderType.StopLimit ? stopPrice : default(double?);
                var maxVisVolume = orderType == OrderType.Limit || orderType == OrderType.StopLimit ? maxVisibleVolume : default(double?);

                TradeProxy.Server.ModifyTradeRecordEx(operationId, orderId, symbol,
                    ToRecordType(orderType), Convert(side), volume, maxVisVolume, px, stopPx, sl, tp, expiration, comment, null, null);
            });
        }

        public void SendCloseOrder(CrossDomainCallback<OrderCmdResultCodes> callback, string operationId, string orderId, double? volume)
        {
            EnqueueTradeOp("CloseOrder", callback, () =>
            {
                ValidateOrderId(orderId);

                if (volume == null)
                {
                    TradeProxy.Server.ClosePositionEx(orderId, operationId);
                }
                else
                {
                    ValidateVolume(volume.Value);
                    TradeProxy.Server.ClosePositionPartiallyEx(orderId, volume.Value, operationId);
                }
            });
        }

        public void SendCloseOrderBy(CrossDomainCallback<OrderCmdResultCodes> callback, string operationId, string orderId, string byOrderId)
        {
            EnqueueTradeOp("CloseOrderBy", callback, () =>
            {
                ValidateOrderId(orderId);
                ValidateOrderId(byOrderId);

                var result = TradeProxy.Server.CloseByPositionsEx(operationId, orderId, byOrderId, -1);
                if (!result)
                    throw new Exception("False! CloseByPositionsEx does not return error code! So enjoy this False by now.");
            });
        }

        private void EnqueueTradeOp(string opName, CrossDomainCallback<OrderCmdResultCodes> callback, Action tradeOpDef)
        {
            EnqueueTask(() =>
            {
                var result = HandleErrors(opName, tradeOpDef);
                callback.Invoke(result);
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
            catch (ConnectionLostException)
            {
                return OrderCmdResultCodes.ConnectionError;
            }
            catch (SoftFX.Extended.Errors.RejectException rex)
            {
                return FdkToAlgo.Convert(rex.Reason, rex.Message);
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

        private void ValidateExpiration(DateTime? expiration)
        {
            if (expiration == null)
                return;

            var epoch = new DateTime(1970, 1, 1, 0, 0, 0);
            if (expiration.Value < epoch)
                throw new ValidationException(OrderCmdResultCodes.IncorrectExpiration);
        }

        private class ValidationException : Exception
        {
            public ValidationException(OrderCmdResultCodes code)
            {
                this.Code = code;
            }

            public OrderCmdResultCodes Code { get; private set; }
        }

        private class ConnectionLostException : Exception { }
    }
}
