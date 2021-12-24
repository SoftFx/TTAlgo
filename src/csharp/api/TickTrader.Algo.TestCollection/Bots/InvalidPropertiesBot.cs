using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    public enum EmptyE { }

    [TradeBot(DisplayName = "[T] Invalid Properties Bot", Version = "1.0", Category = "Test Plugin Setup",
        Description = "Contains an invalid property. Setup should not allow to continue")]
    public class InvalidPropertiesBot : TradeBot
    {
        [Parameter]
        public InvalidPropertiesBot Property1 { get; set; }

        [Parameter]
        protected int Property2 { get; set; }

        [Parameter]
        public int Property3 { get; private set; }

        [Parameter]
        public int Property4 { private get; set; }

        [Parameter]
        [Output]
        public string Property5 { get; set; }

        [Parameter]
        public EmptyE Property6 { get; set; }

        [Input]
        public DataSeries Input1 { get; private set; }

        [Output]
        public DataSeries Output1 { get; set; }
    }

    [TradeBot(DisplayName = "[T] Unsupported Properties Bot", Version = "1.0", Category = "Test Plugin Setup",
        Description = "Contains unsupported properties. Setup should not allow to continue")]
    public class UnsupportedPropertiesBot : TradeBot
    {
        [Parameter]
        public TimeSpan Property1 { get; set; }

        [Parameter]
        public Version Property2 { get; set; }
    }

    [TradeBot(DisplayName = "[T] Unsupported Input Bot", Version = "1.0", Category = "Test Plugin Setup",
        Description = "Contains unsupported input. Setup should not allow to continue")]
    public class UnsupportedInputBot : TradeBot
    {
        [Input]
        public DataSeries<decimal> Input1 { get; set; }
    }
}
