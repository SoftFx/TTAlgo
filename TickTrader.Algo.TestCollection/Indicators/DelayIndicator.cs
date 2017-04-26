using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Indicators
{
    [Indicator(IsOverlay = true, Category = "Test Indicator Routine", DisplayName = "[T] Delay Indicator", Version = "1.0",
        Description = "Waits for specified amount of milliseconds on last bar update")]
    public class DelayIndicator : Indicator
    {
        [Parameter(DisplayName = "Delay (ms)", DefaultValue = 1000)]
        public int Delay { get; set; }

        [Input(DisplayName = "Price")]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "Delayed Price", DefaultColor = Colors.Red)]
        public DataSeries DelayedPrice { get; set; }


        protected override void Calculate()
        {
            if (IsUpdate)
            {
                Task.Delay(Delay).Wait();
            }
            DelayedPrice[0] = Price[0];
        }
    }
}
