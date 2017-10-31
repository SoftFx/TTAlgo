using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Common.Model
{
    public class BarHistoryReport
    {
        public List<BarEntity> Bars { get; }
        public DateTime? From { get; }
        public DateTime? To { get; }
        public DateTime FromAll { get; }
        public DateTime ToAll { get; }
    }
}
