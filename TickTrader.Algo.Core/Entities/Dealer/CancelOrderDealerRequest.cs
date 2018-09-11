using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    internal class CancelOrderDealerRequest : Api.Ext.CancelOrderRequest
    {
        public CancelOrderDealerRequest(OrderAccessor order, Quote currentRate)
        {
            Order = order;
            CurrentRate = currentRate;
            Confirmed = true;
        }

        public Order Order { get; }
        public Quote CurrentRate { get; }

        public bool Confirmed { get; private set; }

        public void Confirm()
        {
            Confirmed = true;
        }

        public void Reject()
        {
            Confirmed = false;
        }
    }
}
