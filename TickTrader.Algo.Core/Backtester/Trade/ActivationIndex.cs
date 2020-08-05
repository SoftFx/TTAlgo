using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    internal class ActivationIndex : SortedList<double, LinkedList<ActivationRecord>>
    {
        private Func<double, double, bool> priceCompareFunc;
        private Func<IRateInfo, double> priceSelectorFunc;
        private Func<IRateInfo, double> aggrPriceSelector;

        public ActivationIndex(IComparer<double> comparer, Func<double, double, bool> priceCompareFunc, Func<IRateInfo, double> priceSelectorFunc,
            Func<IRateInfo, double> aggrPriceSelector)
            : base(comparer)
        {
            this.priceCompareFunc = priceCompareFunc;
            this.priceSelectorFunc = priceSelectorFunc;
            this.aggrPriceSelector = aggrPriceSelector;
        }

        public bool AddRecord(ActivationRecord record, IRateInfo rate)
        {
            LinkedList<ActivationRecord> list;
            if (!TryGetValue(record.Price, out list))
            {
                list = new LinkedList<ActivationRecord>();
                Add(record.Price, list);
            }
            list.AddLast(record);

            if (rate != null)
            {
                double? currentRate = priceSelectorFunc(rate);
                if (currentRate.HasValue && priceCompareFunc(record.Price, currentRate.Value))
                {
                    record.ActivationPrice = currentRate.Value;
                    return true;
                }
            }

            return false;
        }

        public bool RemoveOrder(OrderAccessor order, ActivationType activationType)
        {
            double price = ActivationRecord.GetActivationPrice(order, activationType);

            LinkedList<ActivationRecord> list;
            if (!TryGetValue(price, out list))
                return false;

            ActivationRecord record = list.FirstOrDefault(r => r.OrderId == order.Id);
            if (record == null)
                return false;
            list.Remove(record);

            if (list.Count == 0)
                Remove(price);

            return true;
        }

        public void ResetOrderActivation(OrderAccessor order, ActivationType activationType)
        {
            double price = ActivationRecord.GetActivationPrice(order, activationType);

            LinkedList<ActivationRecord> list;
            if (!TryGetValue(price, out list))
                return;

            ActivationRecord record = list.FirstOrDefault(r => r.OrderId == order.Id);
            if (record == null)
                return;

            record.LastNotifyTime = null;
        }

        public void CheckPendingOrders(IRateInfo rate, List<ActivationRecord> result)
        {
            double currentRate = aggrPriceSelector(rate);

            foreach (double price in Keys)
            {
                if (!priceCompareFunc(price, currentRate))
                    break;

                var records = this[price];

                foreach (var rec in records)
                {
                    rec.ActivationPrice = currentRate;
                    result.Add(rec);
                }
            }
        }
    }
}
