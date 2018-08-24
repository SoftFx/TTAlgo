using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Ext;

namespace TickTrader.Algo.Core
{
    internal class DefaultDealer : DealerEmulator
    {
        public bool ConfirmOrderCancelation(Order order)
        {
            return true;
        }

        public bool ConfirmOrderOpen(Order order, RateUpdate rate, out FillInfo? fill)
        {
            fill = null;
            return true;
        }

        public bool ConfirmOrderReplace(Order order, OrderModifyInfo request)
        {
            return true;
        }
    }
}
