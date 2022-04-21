using TickTrader.Algo.Api;

namespace EmptyIndicator
{
    [Indicator(Category = "My indicators", DisplayName = "EmptyIndicator", Version = "1.0",
        Description = "My awesome EmptyIndicator")]
    public class EmptyIndicator : Indicator
    {
        protected override void Init()
        {
            // TO DO: Put your initialization logic here...
        }

        protected override void Calculate(bool isNewBar)
        {
            // TO DO: Put your calculation logic here...
        }
    }
}
