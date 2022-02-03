using System;

namespace TickTrader.Algo.Backtester
{
    public class TestingStatistics
    {
        public TestingStatistics()
        {
            ProfitByHours = new double[24];
            LossByHours = new double[24];
            ProfitByWeekDays = new double[7];
            LossByWeekDays = new double[7];
        }

        public long BarsCount { get; set; }
        public long TicksCount { get; set; }
        public int OrdersOpened { get; set; }
        public int OrdersRejected { get; set; }
        public int OrderModifications { get; set; }
        public int OrderModificationRejected { get; set; }

        public double ElapsedMs { get; set; }

        public double GrossProfit { get; set; }
        public double GrossLoss { get; set; }
        public double InitialBalance { get; set; }
        public double FinalBalance { get; set; }
        public double TotalComission { get; set; }
        public double TotalSwap { get; set; }
        
        public int AccBalanceDigits { get; set; }

        public double[] ProfitByHours { get; set; }
        public double[] LossByHours { get; set; }
        public double[] ProfitByWeekDays { get; set; }
        public double[] LossByWeekDays { get; set; }
    }
}
