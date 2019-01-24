using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    public class TradeReportKey : IComparable, IComparable<TradeReportKey>
    {
        public TradeReportKey(long orderId, int? actionNo)
        {
            OrderId = orderId;
            ActionNo = actionNo;
        }

        public long OrderId { get; set; }
        public int? ActionNo { get; set; }

        public override string ToString()
        {
            if (ActionNo == null)
                return OrderId.ToString();
            else
                return OrderId + "-" + ActionNo.Value;
        }

        #region IComparable

        public int CompareTo(object obj)
        {
            return CompareTo((TradeReportKey)obj);
        }

        public int CompareTo(TradeReportKey other)
        {
            var timeCmp = OrderId.CompareTo(other.OrderId);
            if (timeCmp != 0)
                return timeCmp;
            return Nullable.Compare(ActionNo, other.ActionNo);
        }

        public override bool Equals(object obj)
        {
            if (obj is TradeReportKey)
            {
                var other = (TradeReportKey)obj;
                return other.OrderId == OrderId && other.ActionNo == ActionNo;
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)2166136261;
                hash = (hash * 16777619) ^ OrderId.GetHashCode();
                hash = (hash * 16777619) ^ ActionNo.GetHashCode();
                return hash;
            }
        }

        #endregion
    }
}
