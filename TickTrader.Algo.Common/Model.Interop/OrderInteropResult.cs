using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Common.Model.Interop
{
    internal class OrderInteropResult
    {
        public OrderInteropResult(OrderCmdResultCodes code, ExecutionReport report = null)
        {
            ResultCode = code;
            Report = report;
        }

        public OrderCmdResultCodes ResultCode { get; }
        public ExecutionReport Report { get; }
    }
}
