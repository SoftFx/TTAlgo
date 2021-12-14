using TickTrader.Algo.Api;

namespace $safeprojectname$
{
    [TradeBot(Category = "My bots", DisplayName = "$projectname$", Version = "1.0",
        Description = "My awesome $projectname$")]
    public class $safeprojectname$ : TradeBot
    {
        [Parameter(DisplayName = "Param 1", DefaultValue = 2)]
        public int IntParam { get; set; }

        [Parameter(DisplayName = "Param 2", DefaultValue = 1.2)]
        public double DoubleParam { get; set; }

        public enum Variants { Varitan1, Variant2, Variant3 }
        [Parameter(DisplayName = "Param 3", DefaultValue = Variants.Variant3)]
        public Variants EnumParam { get; set; }

        [Input]
        public DataSeries PriceInput { get; set; }

        protected override void Init()
        {
            // TO DO: Put your initialization logic here...
        }

        protected override void OnQuote(Quote quote)
        {
            // TO DO:
        }
    }
}
