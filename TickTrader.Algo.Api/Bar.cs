using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public struct Bar
    {
        public double Open { get; set; }
        public double Close { get; set; }
        public double Hi { get; set; }
        public double Lo { get; set; }
        public DateTime OpenTime { get; set; }
    }
}
