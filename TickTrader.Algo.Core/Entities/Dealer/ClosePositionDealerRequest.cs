using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    internal class ClosePositionDealerRequest : Api.Ext.ClosePositionRequest
    {
        public ClosePositionDealerRequest(OrderAccessor pos, Quote currentRate)
        {
            Position = pos;
            CurrentRate = currentRate;
            Confirmed = true;
        }

        public double? CloseAmount { get; set; }
        public Order Position { get; }
        public Quote CurrentRate { get; }

        public bool Confirmed { get; private set; }
        public double? DealerPrice { get; private set; }

        public void Confirm()
        {
            Confirmed = true;
        }

        public void Confirm(double price)
        {
            DealerPrice = price;
            Confirmed = true;
        }

        public void Reject()
        {
            Confirmed = false;
        }
    }
}
