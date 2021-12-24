using TickTrader.Algo.Api;
using TickTrader.Algo.TestCollection.Indicators;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Nested Print Indicator Bot", Version = "1.0", Category = "Test Bot Routine",
        Description = "Uses Print Indicator as nested")]
    public class NestedPrintIndicatorBot : TradeBot
    {
        private PrintIndicator _printIndicator;


        [Parameter(DisplayName = "Count Records", DefaultValue = 100)]
        public int Count { get; set; }

        [Parameter(DisplayName = "Use Count", DefaultValue = true)]
        public bool UseCount { get; set; }


        protected override void Init()
        {
            _printIndicator = new PrintIndicator() { Count = Count, UseCount = UseCount };
        }
    }
}
