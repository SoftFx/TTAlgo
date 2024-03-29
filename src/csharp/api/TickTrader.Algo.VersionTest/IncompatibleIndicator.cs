﻿using TickTrader.Algo.Api;

namespace TickTrader.Algo.VersionTest
{
    [Indicator(Category = "Test Plugin Setup", DisplayName = "Incompatible Indicator", Version = "1.0",
        Description = "Should display a warning that newer version of API was used to build this indicator")]
    public class IncompatibleIndicator : Indicator
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

        [Output(DisplayName = "Line 1", Target = OutputTargets.Overlay, DefaultColor = Colors.LightSeaGreen)]
        public DataSeries Ouput1 { get; set; }

        [Output(DisplayName = "Line 2", Target = OutputTargets.Window1, DefaultColor = Colors.Red)]
        public DataSeries Ouput2 { get; set; }

        protected override void Init()
        {
        }

        protected override void Calculate()
        {

        }
    }
}
