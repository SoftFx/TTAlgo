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
        public OrderInteropResult(OrderCmdResultCodes code, List<ExecutionReport> reports = null)
        {
            ResultCode = code;
            Reports = reports;
        }

        public OrderInteropResult(OrderCmdResultCodes code, ExecutionReport report)
        {
            ResultCode = code;
            if (report != null)
            {
                Reports = new List<ExecutionReport>();
                Reports.Add(report);
            }
        }

        public OrderCmdResultCodes ResultCode { get; }
        public List<ExecutionReport> Reports { get; }
    }
}
