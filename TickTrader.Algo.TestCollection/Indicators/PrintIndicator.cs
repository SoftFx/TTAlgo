using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Indicators
{
    [Indicator(DisplayName = "[T] Print Indicator", Version = "1.0", Category = "Test Indicator Routine")]
    public class PrintIndicator : Indicator
    {
        private int _count = 0;


        [Parameter(DisplayName = "Count Records", DefaultValue = 100)]
        public int Count { get; set; }

        [Parameter(DisplayName = "Use Count", DefaultValue = true)]
        public bool UseCount { get; set; }


        protected override void Calculate(bool isNewBar)
        {
            if (!isNewBar)
            {
                if (UseCount && _count < Count)
                {
                    Print($"Indicator info #{++_count}");
                    PrintError($"Indicator error #{++_count}");
                }
            }
        }
    }
}
