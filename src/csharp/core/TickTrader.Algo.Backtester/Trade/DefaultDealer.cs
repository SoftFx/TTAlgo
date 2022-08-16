using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Ext;

namespace TickTrader.Algo.Backtester
{
    internal class DefaultDealer : DealerEmulator
    {
        public void ConfirmOrderCancelation(Api.Ext.CancelOrderRequest request)
        {
            request.Confirm();
        }

        public void ConfirmOrderOpen(Api.Ext.OpenOrderRequest request)
        {
            var order = request.Order;

            if (order.Type == OrderType.Limit && order.Options.HasFlag(OrderOptions.ImmediateOrCancel))
            {
                var quote = request.CurrentRate;

                // IOC
                if (order.Side == OrderSide.Buy && quote.HasAsk && (quote.Ask <= order.Price))
                {
                    request.Confirm(order.RequestedVolume, quote.Ask);
                    return;
                }

                if (order.Side == OrderSide.Sell && quote.HasBid && (quote.Bid >= order.Price))
                {
                    request.Confirm(order.RequestedVolume, quote.Bid);
                    return;
                }

                request.Reject();

                return;
            }

            // Market

            //if (order.Type == OrderType.Market)
            //{
            //    if ((Request.Side == OrderSides.Buy) && currentQuote.NullableAsk.HasValue)
            //    {
            //        dParams.Price = currentQuote.NullableAsk.Value;
            //        return true;
            //    }
            //    else if ((Request.Side == OrderSides.Sell) && currentQuote.NullableBid.HasValue)
            //    {
            //        dParams.Price = currentQuote.NullableBid.Value;
            //        return true;
            //    }
            //}

            request.Confirm();
        }

        public void ConfirmOrderReplace(Api.Ext.ModifyOrderRequest request)
        {
            request.Confirm();
        }

        public void ConfirmPositionClose(Api.Ext.ClosePositionRequest request)
        {
            request.Confirm();
        }
    }
}
