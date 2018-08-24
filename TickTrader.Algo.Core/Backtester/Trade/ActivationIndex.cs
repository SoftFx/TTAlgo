using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.BusinessObjects;

namespace TickTrader.Algo.Core
{
    internal class ActivationIndex : SortedList<decimal, LinkedList<ActivationRecord>>
    {
        private Func<decimal, decimal, bool> priceCompareFunc;
        private Func<RateUpdate, decimal?> priceSelectorFunc;
        private Func<RateUpdate, decimal> aggrPriceSelector;

        public ActivationIndex(IComparer<decimal> comparer, Func<decimal, decimal, bool> priceCompareFunc, Func<RateUpdate, decimal?> priceSelectorFunc,
            Func<RateUpdate, decimal> aggrPriceSelector)
            : base(comparer)
        {
            this.priceCompareFunc = priceCompareFunc;
            this.priceSelectorFunc = priceSelectorFunc;
            this.aggrPriceSelector = aggrPriceSelector;
        }

        public bool AddRecord(ActivationRecord record, RateUpdate smbInfo)
        {
            LinkedList<ActivationRecord> list;
            if (!TryGetValue(record.Price, out list))
            {
                list = new LinkedList<ActivationRecord>();
                Add(record.Price, list);
            }
            list.AddLast(record);

            if (smbInfo != null)
            {
                decimal? currentRate = priceSelectorFunc(smbInfo);
                if (currentRate.HasValue && priceCompareFunc(record.Price, currentRate.Value))
                {
                    record.ActivationPrice = currentRate.Value;
                    return true;
                }
            }

            return false;
        }

        public bool RemoveOrder(OrderAccessor order, ActivationTypes activationType)
        {
            decimal price = ActivationRecord.GetActivationPrice(order, activationType);

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

        public void ResetOrderActivation(OrderAccessor order, ActivationTypes activationType)
        {
            decimal price = ActivationRecord.GetActivationPrice(order, activationType);

            LinkedList<ActivationRecord> list;
            if (!TryGetValue(price, out list))
                return;

            ActivationRecord record = list.FirstOrDefault(r => r.OrderId == order.Id);
            if (record == null)
                return;

            record.LastNotifyTime = null;
        }

        public IEnumerable<ActivationRecord> CheckPendingOrders(RateUpdate smbInfo)
        {
            List<decimal> affectedPrices = new List<decimal>();
            List<ActivationRecord> result = new List<ActivationRecord>();

            decimal currentRate = aggrPriceSelector(smbInfo);

            foreach (decimal price in Keys)
            {
                if (!priceCompareFunc(price, currentRate))
                    break;
                affectedPrices.Add(price);
            }

            foreach (decimal affectedPrice in affectedPrices)
                result.AddRange(this[affectedPrice]);

            foreach (ActivationRecord record in result)
                record.ActivationPrice = currentRate;

            return result;
        }
    }
}
