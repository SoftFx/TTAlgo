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
        public List<BarEntity> Bars { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public DateTime FromAll { get; set; }
        public DateTime ToAll { get; set; }
    }
}
