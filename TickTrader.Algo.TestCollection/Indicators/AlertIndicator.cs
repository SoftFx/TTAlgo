using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Indicators
{
    [Indicator(Category = "Test Indicator Routine", DisplayName = "[T] Alert Indicator", Version = "1.0")]
    public class AlertIndicator : Indicator
    {
        [Parameter(DisplayName = "Delay (ms)", DefaultValue = 100)]
        public int DelayMc { get; set; }

        [Parameter(DisplayName = "Count Records", DefaultValue = 100)]
        public int Count { get; set; }

        [Parameter(DisplayName = "Use Count", DefaultValue = true)]
        public bool UseCount { get; set; }

        private int _count = 0;

        protected override async void Init()
        {
            while (true)
            {
                await Task.Delay(DelayMc);
                Alert.Print($"I'm alert indicator. {++_count}");

                if (UseCount && _count >= Count)
                    break;
            }

        }
    }
}
