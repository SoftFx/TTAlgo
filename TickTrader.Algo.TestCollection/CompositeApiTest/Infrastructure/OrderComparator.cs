using System;
using System.Collections.Generic;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal sealed class OrderComparator
    {
        private static readonly Type _originType;

        private readonly Order _originOrder;


        static OrderComparator()
        {
            _originType = typeof(Order);
        }

        private OrderComparator(Order origin, OrderTemplate template)
        {
            _originOrder = origin;

            CheckOriginalValue(template == null, nameof(Order.IsNull));
            CheckOriginalValue(TestParamsSet.Symbol.Name, nameof(Order.Symbol));

            CheckOriginalValue(template.Id, nameof(Order.Id));
            CheckOriginalValue(template.Type, nameof(Order.Type));
            CheckOriginalValue(template.Side, nameof(Order.Side));

            CheckOriginalValue(template.Volume, nameof(Order.RemainingVolume));

            CheckOriginalValue(template.Comment, nameof(Order.Comment));
            CheckOriginalValue(template.Tag, nameof(Order.Tag));

            //if (close || IsInstantOrder || Type == OrderType.Position) //to do: create verification for Position
            //    return;

            //if (RealOrder.IsNull != close)
            //    throw new VerificationException(Id, close);

            //if (RealOrder.Type != Type)
            //    ThrowVerificationException(nameof(RealOrder.Type), Type, RealOrder.Type);

            //if (RealOrder.Side != Side)
            //    ThrowVerificationException(nameof(RealOrder.Side), Side, RealOrder.Side);

            //if (!RealOrder.RemainingVolume.EI(Volume))
            //    ThrowVerificationException(nameof(RealOrder.RemainingVolume), Volume, RealOrder.RemainingVolume);

            //if (Type != OrderType.Stop && !CheckPrice(RealOrder.Price))
            //    ThrowVerificationException(nameof(RealOrder.Price), Price, RealOrder.Price);

            //if (IsSupportedStopPrice && !RealOrder.StopPrice.EI(StopPrice))
            //    ThrowVerificationException(nameof(RealOrder.StopPrice), StopPrice, RealOrder.StopPrice);

            //if (!RealOrder.MaxVisibleVolume.EI(MaxVisibleVolume) && !(-1.0).EI(MaxVisibleVolume) && !double.IsNaN(RealOrder.MaxVisibleVolume))
            //    ThrowVerificationException(nameof(RealOrder.MaxVisibleVolume), MaxVisibleVolume, RealOrder.MaxVisibleVolume);

            //if (!RealOrder.TakeProfit.EI(TP) && !0.0.EI(TP) && !double.IsNaN(RealOrder.TakeProfit))
            //    ThrowVerificationException(nameof(RealOrder.TakeProfit), TP, RealOrder.TakeProfit);

            //if (!RealOrder.StopLoss.EI(SL) && !0.0.EI(SL) && !double.IsNaN(RealOrder.StopLoss))
            //    ThrowVerificationException(nameof(RealOrder.StopLoss), SL, RealOrder.StopLoss);

            //if (IsSupportedSlippage)
            //    CheckSlippage(RealOrder.Slippage, (realSlippage, expectedSlippage) =>
            //    {
            //        if (!realSlippage.E(expectedSlippage))
            //            ThrowVerificationException(nameof(RealOrder.Slippage), expectedSlippage, realSlippage);
            //    });

            //if (IsSupportedOcO)
            //    if (OcoRelatedOrderId != RealOrder.OcoRelatedOrderId)
            //        ThrowVerificationException(nameof(RealOrder.OcoRelatedOrderId), OcoRelatedOrderId, RealOrder.OcoRelatedOrderId);

            //if (Comment != null && RealOrder.Comment != Comment)
            //    ThrowVerificationException(nameof(RealOrder.Comment), Comment, RealOrder.Comment);

            //if (Expiration != null && RealOrder.Expiration != Expiration)
            //    ThrowVerificationException(nameof(RealOrder.Expiration), Expiration, RealOrder.Expiration);


            //if (RealOrder.Tag != Tag)
            //    ThrowVerificationException(nameof(RealOrder.Tag), Tag, RealOrder.Tag);
        }

        //private bool CheckPrice(double cur)
        //{
        //    if (IsInstantOrder || Type == OrderType.Position)
        //        return Side == OrderSide.Buy ? cur.LteI(Price) : cur.GteI(Price);

        //    return cur.EI(Price);
        //}


        internal static void Compare(Order originalOrder, OrderTemplate template) => new OrderComparator(originalOrder, template);

        internal static void CompareWithRealOrder(OrderTemplate template) => new OrderComparator(template.RealOrder.DeepCopy(), template);


        private void CheckOriginalValue<T>(T expectedValue, string propertyName)
        {
            var originValue = (T)_originType.GetProperty(propertyName).GetValue(_originOrder);

            if (Comparer<T>.Default.Compare(originValue, expectedValue) != 0)
                throw new VerificationException(_originOrder.Id, propertyName, expectedValue, originValue);
        }
    }
}