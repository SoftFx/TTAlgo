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
        public DealerResponse ConfirmOrderOpen(Order order, RateUpdate rate)
        {
            return new DealerResponse();
        }
    }
}
