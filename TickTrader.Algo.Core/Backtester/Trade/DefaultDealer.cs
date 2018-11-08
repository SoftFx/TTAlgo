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
        public void ConfirmOrderCancelation(Api.Ext.CancelOrderRequest request)
        {
            request.Confirm();
        }

        public void ConfirmOrderOpen(Api.Ext.OpenOrderRequest request)
        {
            request.Confirm();
        }

        public void ConfirmOrderReplace(ModifyOrderRequest request)
        {
            request.Confirm();
        }

        public void ConfirmPositionClose(ClosePositionRequest request)
        {
            request.Confirm();
        }
    }
}
