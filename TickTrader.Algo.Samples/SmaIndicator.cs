using TickTrader.Algo.Api;

namespace TickTrader.Algo.Samples
{
    [Indicator]
    public class SmaIndicator : Indicator
    {
        [Input]
        public DataSeries Input { get; set; }

        [Output]
        public DataSeries Output { get; set; }

        [Parameter]
        public int Depth { get; set; }

        protected override void Calculate(bool isNewBar)
        {
        }
    }
}
