using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class TestingStatistics
    {
        public TestingStatistics()
        {
            ProfitByHours = new decimal[24];
            LossByHours = new decimal[24];
            ProfitByWeekDays = new decimal[7];
            LossByWeekDays = new decimal[7];
        }

        public long BarsCount { get; internal set; }
        public long TicksCount { get; internal set; }
        public int OrdersOpened { get; internal set; }
        public int OrdersRejected { get; internal set; }
        public int OrderModifications { get; internal set; }
        public int OrderModificationRejected { get; internal set; }

        public TimeSpan Elapsed { get; internal set; }

        public decimal GrossProfit { get; internal set; }
        public decimal GrossLoss { get; internal set; }
        public decimal InitialBalance { get; internal set; }
        public decimal FinalBalance { get; internal set; }

        public decimal[] ProfitByHours { get; }
        public decimal[] LossByHours { get; }
        public decimal[] ProfitByWeekDays { get; }
        public decimal[] LossByWeekDays { get; }
    }
}
