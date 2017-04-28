using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.VersionTest
{
    [Indicator(DisplayName = "Incompatible Indicator")]
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

        [Output(DisplayName = "Line 1", DefaultColor = Colors.LightSeaGreen)]
        public DataSeries Ouput1 { get; set; }

        [Output(DisplayName = "Line 2", DefaultColor = Colors.Red)]
        public DataSeries Ouput2 { get; set; }

        protected override void Init()
        {
        }

        protected override void Calculate()
        {

        }
    }
}
