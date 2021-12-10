using TickTrader.Algo.Api;

namespace SampleIndicator
{
    [Indicator(Category = "My indicators", DisplayName = "SampleIndicator", Version="1.0",
        Description = "My own SampleIndicator")]
    public class SampleIndicator : Indicator
    {
        protected override void Calculate(bool isNewBar)
        {
        }
    }
}
