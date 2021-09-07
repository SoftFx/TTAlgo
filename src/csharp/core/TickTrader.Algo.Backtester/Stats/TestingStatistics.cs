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

        public long BarsCount { get; internal set; }
        public long TicksCount { get; internal set; }
        public int OrdersOpened { get; internal set; }
        public int OrdersRejected { get; internal set; }
        public int OrderModifications { get; internal set; }
        public int OrderModificationRejected { get; internal set; }

        public TimeSpan Elapsed { get; internal set; }

        public double GrossProfit { get; internal set; }
        public double GrossLoss { get; internal set; }
        public double InitialBalance { get; internal set; }
        public double FinalBalance { get; internal set; }
        public double TotalComission { get; internal set; }
        public double TotalSwap { get; internal set; }
        
        public int AccBalanceDigits { get; internal set; }

        public double[] ProfitByHours { get; }
        public double[] LossByHours { get; }
        public double[] ProfitByWeekDays { get; }
        public double[] LossByWeekDays { get; }
    }
}
