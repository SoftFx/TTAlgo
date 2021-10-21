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
        }


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